#nullable enable
using net.puk06.ColorChanger.Editor.Extentions;
using net.puk06.ColorChanger.Editor.Services;
using net.puk06.ColorChanger.Models;
using net.puk06.ColorChanger.Utils;
using System;
using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Service
{
    internal class ImageProcessor
    {
        private Color _sourceColor = Color.white;
        private Color _targetColor = Color.white;

        private bool _isBalanceMode = false;
        private BalanceModeConfiguration _balanceModeConfiguration = new();

        private bool _isAdvancedColorMode = false;
        private AdvancedColorConfiguration _advancedColorConfiguration = new();

        internal ImageProcessor(ColorChangerForUnity component)
        {
            _sourceColor = component.SourceColor;
            _targetColor = component.TargetColor;
        }

        internal void SetBalanceSettings(BalanceModeConfiguration balanceModeConfiguration)
        {
            _balanceModeConfiguration = balanceModeConfiguration;
            _isBalanceMode = true;
        }

        internal void SetColorSettings(AdvancedColorConfiguration advancedColorConfiguration)
        {
            _advancedColorConfiguration = advancedColorConfiguration;
            _isAdvancedColorMode = true;
        }

        internal void ProcessRenderTexture(RenderTexture source, RenderTexture target, RenderTexture? mask, ImageMaskSelectionType imageMaskSelectionType = ImageMaskSelectionType.None)
        {
            if (source == null || target == null) return;
            
            Texture2D? gradientTexture = null;

            try
            {
                static int convertColorToInt32(float colorValue) => Mathf.RoundToInt(colorValue * 255f);
                static int[] toInt32Array(Color color) => new int[4] { convertColorToInt32(color.r), convertColorToInt32(color.g), convertColorToInt32(color.b), convertColorToInt32(color.a) };

                ComputeShader? colorComputeShader = CCShaderEngine.TextureProcessorComputeShader;
                if (colorComputeShader == null)
                {
                    LogUtils.LogError("The compute shader file could not be found.");
                    return;
                }

                int kernel = colorComputeShader.FindKernel("CSMain");

                colorComputeShader.SetTexture(kernel, "_SourceTex", source);
                colorComputeShader.SetTexture(kernel, "_TargetTex", target);

                colorComputeShader.SetBool("_UseMask", mask != null && imageMaskSelectionType != ImageMaskSelectionType.None);
                colorComputeShader.SetTexture(kernel, "_MaskTex", mask != null ? mask : DummyRenderTexture.Instance);

                Vector2 maskScale = mask == null ? new() : new Vector2((float)mask.width / source.width, (float)mask.height / source.height);
                colorComputeShader.SetVector("_MaskTexScale", maskScale);
                colorComputeShader.SetInt("_MaskSelectionType", (int)imageMaskSelectionType);

                colorComputeShader.SetInts("_SourceColor", toInt32Array(_sourceColor));
                colorComputeShader.SetInts("_TargetColor", toInt32Array(_targetColor));

                // バランスモード
                colorComputeShader.SetBool("_BalanceModeEnabled", _isBalanceMode);
                colorComputeShader.SetInt("_BalanceModeVersion", _balanceModeConfiguration.ModeVersion);
                colorComputeShader.SetFloat("_BalanceModeV1Weight", _balanceModeConfiguration.V1Weight);
                colorComputeShader.SetFloat("_BalanceModeV1MinimumValue", _balanceModeConfiguration.V1MinimumValue);
                colorComputeShader.SetFloat("_BalanceModeV2Weight", _balanceModeConfiguration.V2Weight);
                colorComputeShader.SetFloat("_BalanceModeV2Radius", _balanceModeConfiguration.V2Radius);
                colorComputeShader.SetFloat("_BalanceModeV2MinimumValue", _balanceModeConfiguration.V2MinimumValue);
                colorComputeShader.SetBool("_BalanceModeV2IncludeOutside", _balanceModeConfiguration.V2IncludeOutside);

                gradientTexture = _balanceModeConfiguration.V3Gradient.ToTexture(_balanceModeConfiguration.V3GradientResolution);
                colorComputeShader.SetTexture(kernel, "_BalanceModeV3Gradient", gradientTexture);
                colorComputeShader.SetInt("_BalanceModeV3GradientResolution", _balanceModeConfiguration.V3GradientResolution);

                // 追加設定
                colorComputeShader.SetBool("_AdvancedColorModeEnabled", _isAdvancedColorMode);
                colorComputeShader.SetFloat("_AdvancedColorSettingsBrightness", _advancedColorConfiguration.Brightness);
                colorComputeShader.SetFloat("_AdvancedColorSettingsContrast", _advancedColorConfiguration.Contrast);
                colorComputeShader.SetFloat("_AdvancedColorSettingsGamma", _advancedColorConfiguration.Gamma);
                colorComputeShader.SetFloat("_AdvancedColorSettingsExposure", _advancedColorConfiguration.Exposure);
                colorComputeShader.SetFloat("_AdvancedColorSettingsTransparency", _advancedColorConfiguration.Transparency);

                int threadGroupX = Mathf.CeilToInt(source.width / 16.0f);
                int threadGroupY = Mathf.CeilToInt(source.height / 16.0f);
                colorComputeShader.Dispatch(kernel, threadGroupX, threadGroupY, 1);
            }
            catch (Exception ex)
            {
                LogUtils.LogError($"Failed to compute shader.\n{ex}");
            }

            if (gradientTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(gradientTexture);
            }
        }
    }
}
