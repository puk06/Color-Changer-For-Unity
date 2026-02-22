#nullable enable
using net.puk06.ColorChanger.Editor.Extension;
using net.puk06.ColorChanger.Editor.Models;
using net.puk06.ColorChanger.Editor.Utils;
using net.puk06.ColorChanger.Models;
using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Services
{
    internal static class TextureBuilder
    {
        internal static ExtendedRenderTexture? Build(Texture2D sourceTexture, ColorChangerForUnity component, bool useMask)
        {
            if (!component.gameObject.activeInHierarchy) return null;

            if (!ExtendedRenderTexture.TryCreate(sourceTexture.width, sourceTexture.height, out ExtendedRenderTexture originalRenderTexture))
                return null;
            originalRenderTexture.Copy(sourceTexture);

            using (originalRenderTexture)
            {
                ExtendedRenderTexture? maskRenderTexture = null;
                if (useMask && component.MaskTexture != null)
                {
                    if (!ExtendedRenderTexture.TryCreate(component.MaskTexture.width, component.MaskTexture.height, out maskRenderTexture))
                        return null;
                    maskRenderTexture.Copy(component.MaskTexture);
                }

                using (maskRenderTexture)
                {
                    return Process(component, originalRenderTexture, maskRenderTexture, component.ImageMaskSelectionType);
                }
            }
        }

        internal static ExtendedRenderTexture? Process(ColorChangerForUnity component, RenderTexture sourceTexture, RenderTexture? maskTexture, ImageMaskSelectionType imageMaskSelectionType = ImageMaskSelectionType.None, RenderTextureReadWrite readWrite = RenderTextureReadWrite.sRGB)
        {
            ComputeShader? processorShader = CCShaderEngine.TextureProcessorComputeShader;
            if (processorShader == null) CCShaderEngine.LoadShaders();

            if (processorShader == null)
            {
                LogUtils.LogError("Failed to load processor compute shader.");
                return null;
            }

            if (!ExtendedRenderTexture.TryCreate(sourceTexture.width, sourceTexture.height, readWrite, out ExtendedRenderTexture targetTexture))
                return null;

            static int convertColorToInt32(float colorValue) => Mathf.RoundToInt(colorValue * 255f);
            static int[] toInt32Array(Color color) => new int[4] { convertColorToInt32(color.r), convertColorToInt32(color.g), convertColorToInt32(color.b), convertColorToInt32(color.a) };

            int kernel = processorShader.FindKernel("CSMain");

            processorShader.SetTexture(kernel, "_SourceTex", sourceTexture);
            processorShader.SetTexture(kernel, "_TargetTex", targetTexture);

            processorShader.SetBool("_UseMask", maskTexture != null && imageMaskSelectionType != ImageMaskSelectionType.None);
            processorShader.SetTexture(kernel, "_MaskTex", maskTexture != null ? maskTexture : DummyRenderTexture.Instance);

            Vector2 maskScale = maskTexture == null ? new() : new Vector2((float)maskTexture.width / sourceTexture.width, (float)maskTexture.height / sourceTexture.height);
            processorShader.SetVector("_MaskTexScale", maskScale);
            processorShader.SetInt("_MaskSelectionType", (int)imageMaskSelectionType);

            processorShader.SetInts("_SourceColor", toInt32Array(component.SourceColor));
            processorShader.SetInts("_TargetColor", toInt32Array(component.TargetColor));

            processorShader.SetInt("_BalanceModeVersion", component.BalanceModeConfiguration.ModeVersion);
            processorShader.SetFloat("_BalanceModeV1Weight", component.BalanceModeConfiguration.V1Weight);
            processorShader.SetFloat("_BalanceModeV1MinimumValue", component.BalanceModeConfiguration.V1MinimumValue);
            processorShader.SetFloat("_BalanceModeV2Weight", component.BalanceModeConfiguration.V2Weight);
            processorShader.SetFloat("_BalanceModeV2Radius", component.BalanceModeConfiguration.V2Radius);
            processorShader.SetFloat("_BalanceModeV2MinimumValue", component.BalanceModeConfiguration.V2MinimumValue);
            processorShader.SetBool("_BalanceModeV2IncludeOutside", component.BalanceModeConfiguration.V2IncludeOutside);

            Texture2D? v3GradientTexture = component.BalanceModeConfiguration.V3Gradient.ToTexture(component.BalanceModeConfiguration.V3GradientResolution);
            processorShader.SetTexture(kernel, "_BalanceModeV3Gradient", v3GradientTexture);
            processorShader.SetInt("_BalanceModeV3GradientResolution", component.BalanceModeConfiguration.V3GradientResolution);

            processorShader.SetBool("_AdvancedColorModeEnabled", component.AdvancedColorConfiguration.IsEnabled);
            processorShader.SetFloat("_AdvancedColorSettingsHue", component.AdvancedColorConfiguration.Hue / 360f);
            processorShader.SetFloat("_AdvancedColorSettingsSaturation", component.AdvancedColorConfiguration.Saturation / 100f);
            processorShader.SetFloat("_AdvancedColorSettingsValue", component.AdvancedColorConfiguration.Value / 100f);
            processorShader.SetFloat("_AdvancedColorSettingsBrightness", component.AdvancedColorConfiguration.Brightness);
            processorShader.SetFloat("_AdvancedColorSettingsContrast", component.AdvancedColorConfiguration.Contrast);
            processorShader.SetFloat("_AdvancedColorSettingsGamma", component.AdvancedColorConfiguration.Gamma);
            processorShader.SetFloat("_AdvancedColorSettingsExposure", component.AdvancedColorConfiguration.Exposure);
            processorShader.SetFloat("_AdvancedColorSettingsTransparency", component.AdvancedColorConfiguration.Transparency);

            int threadGroupX = Mathf.CeilToInt(sourceTexture.width / 16.0f);
            int threadGroupY = Mathf.CeilToInt(sourceTexture.height / 16.0f);
            processorShader.Dispatch(kernel, threadGroupX, threadGroupY, 1);

            Object.DestroyImmediate(v3GradientTexture);

            return targetTexture;
        }
    }
}
