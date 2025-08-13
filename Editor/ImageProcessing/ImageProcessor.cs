using net.puk06.ColorChanger.Models;
using net.puk06.ColorChanger.Utils;
using Unity.Collections;
using UnityEngine;
using MathUtils = net.puk06.ColorChanger.Utils.MathUtils;

namespace net.puk06.ColorChanger.ImageProcessing
{
    internal class ImageProcessor
    {
        private readonly ColorDifference _colorOffset;

        private bool _isBalanceMode = false;
        private BalanceModeConfiguration _balanceModeConfiguration = new();

        private bool _isAdvancedColorMode = false;
        private AdvancedColorConfiguration _advancedColorConfiguration = new();

        internal ImageProcessor(ColorDifference colorDifference)
        {
            _colorOffset = colorDifference;
        }

        private Color32 ProcessPixel(Color32 source)
        {
            if (_isBalanceMode)
            {
                source = ColorUtils.BalanceColorAdjustment(source, _colorOffset, _balanceModeConfiguration);
            }
            else
            {
                source.r = MathUtils.ClampColorValue(source.r + _colorOffset.DiffR);
                source.g = MathUtils.ClampColorValue(source.g + _colorOffset.DiffG);
                source.b = MathUtils.ClampColorValue(source.b + _colorOffset.DiffB);
            }

            if (_isAdvancedColorMode)
                source = ColorUtils.AdvancedColorAdjustment(source, _advancedColorConfiguration);

            return source;
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

        internal void ProcessAllPixels(Texture2D source, Texture2D target)
        {
            NativeArray<Color32> rawData = source.GetRawTextureData<Color32>();
            NativeArray<Color32> targetData = target.GetRawTextureData<Color32>();

            for (int i = 0; i < targetData.Length; i++)
            {
                targetData[i] = ProcessPixel(rawData[i]);
            }

            target.Apply();
        }

        internal void ProcessAllPixelsGPU(RenderTexture source, RenderTexture target)
        {
            ComputeShader colorComputeShader = ShaderUtils.GetComputeShader();
            if (colorComputeShader == null)
            {
                Debug.LogError("計算用シェーダーファイルが見つかりませんでした。");
                return;
            }

            int kernel = colorComputeShader.FindKernel("CSMain");

            colorComputeShader.SetTexture(kernel, "_Source", source);
            colorComputeShader.SetTexture(kernel, "_Target", target);

            colorComputeShader.SetVector("_prevColor", (Vector4)_colorOffset.PreviousColor);
            colorComputeShader.SetVector("_ColorOffset", _colorOffset.ToVector3());

            // バランスモード
            colorComputeShader.SetBool("_balanceModeEnabled", _isBalanceMode);
            colorComputeShader.SetInt("_balanceModeVersion", _balanceModeConfiguration.ModeVersion);
            colorComputeShader.SetFloat("_balanceModeV1Weight", _balanceModeConfiguration.V1Weight);
            colorComputeShader.SetFloat("_balanceModeV1MinimumValue", _balanceModeConfiguration.V1MinimumValue);
            colorComputeShader.SetFloat("_balanceModeV2Weight", _balanceModeConfiguration.V2Weight);
            colorComputeShader.SetFloat("_balanceModeV2Radius", _balanceModeConfiguration.V2Radius);
            colorComputeShader.SetFloat("_balanceModeV2MinimumValue", _balanceModeConfiguration.V2MinimumValue);
            colorComputeShader.SetBool("_balanceModeV2IncludeOutside", _balanceModeConfiguration.V2IncludeOutside);

            var gradientRenderTexture = GradientToRenderTexture(_balanceModeConfiguration.V3GradientColor, _balanceModeConfiguration.V3GradientPreviewResolution);
            colorComputeShader.SetTexture(kernel, "_balanceModeV3Gradient", gradientRenderTexture);
            colorComputeShader.SetInt("_balanceModeV3GradientResolution", _balanceModeConfiguration.V3GradientPreviewResolution);

            // 追加設定
            colorComputeShader.SetBool("_advancedColorModeEnabled", _isAdvancedColorMode);
            colorComputeShader.SetFloat("_advancedColorSettingsBrightness", _advancedColorConfiguration.Brightness);
            colorComputeShader.SetFloat("_advancedColorSettingsContrast", _advancedColorConfiguration.Contrast);
            colorComputeShader.SetFloat("_advancedColorSettingsGamma", _advancedColorConfiguration.Gamma);
            colorComputeShader.SetFloat("_advancedColorSettingsExposure", _advancedColorConfiguration.Exposure);
            colorComputeShader.SetFloat("_advancedColorSettingsTransparency", _advancedColorConfiguration.Transparency);

            int threadGroupX = Mathf.CeilToInt(source.width / 16.0f);
            int threadGroupY = Mathf.CeilToInt(source.height / 16.0f);
            colorComputeShader.Dispatch(kernel, threadGroupX, threadGroupY, 1);

            if (RenderTexture.active == gradientRenderTexture) RenderTexture.active = null;
            gradientRenderTexture.DiscardContents();
            Object.DestroyImmediate(gradientRenderTexture);
        }

        private RenderTexture GradientToRenderTexture(Gradient gradient, int resolution = 256)
        {
            var tex2D = new Texture2D(resolution, 1, TextureFormat.RGBA32, false);
            tex2D.wrapMode = TextureWrapMode.Clamp;

            for (int i = 0; i < resolution; i++)
            {
                float t = i / (float)(resolution - 1);
                Color col = gradient.Evaluate(t);
                tex2D.SetPixel(i, 0, col);
            }
            tex2D.Apply();

            var renderTexture = new RenderTexture(resolution, 1, 0, RenderTextureFormat.ARGB32);
            renderTexture.enableRandomWrite = true;
            renderTexture.wrapMode = TextureWrapMode.Clamp;
            renderTexture.Create();

            Graphics.Blit(tex2D, renderTexture);

            Object.DestroyImmediate(tex2D);

            return renderTexture;
        }
    }
}
