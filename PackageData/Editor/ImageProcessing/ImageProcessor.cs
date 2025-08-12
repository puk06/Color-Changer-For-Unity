using net.puk06.ColorChanger.Models;
using net.puk06.ColorChanger.Utils;
using System;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using MathUtils = net.puk06.ColorChanger.Utils.MathUtils;

namespace net.puk06.ColorChanger.ImageProcessing
{
    public class ImageProcessor
    {
        private readonly ColorDifference _colorOffset;

        private bool _isBalanceMode = false;
        private BalanceModeConfiguration _balanceModeConfiguration = new();

        private bool _isAdvancedColorMode = false;
        private AdvancedColorConfiguration _advancedColorConfiguration = new();

        public ImageProcessor(ColorDifference colorDifference)
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

        public void SetBalanceSettings(BalanceModeConfiguration balanceModeConfiguration)
        {
            _balanceModeConfiguration = balanceModeConfiguration;
            _isBalanceMode = true;
        }

        public void SetColorSettings(AdvancedColorConfiguration advancedColorConfiguration)
        {
            _advancedColorConfiguration = advancedColorConfiguration;
            _isAdvancedColorMode = true;
        }

        public void ProcessAllPixels(Texture2D source, Texture2D target)
        {
            NativeArray<Color32> rawData = source.GetRawTextureData<Color32>();
            NativeArray<Color32> targetData = target.GetRawTextureData<Color32>();

            for (int i = 0; i < targetData.Length; i++)
            {
                targetData[i] = ProcessPixel(rawData[i]);
            }

            target.Apply();
        }

        const string shaderPath = "Packages/net.puk06.color-changer/Editor/Shader/ColorProcess.compute";
        ComputeShader colorComputeShader;

        public void ProcessAllPixelsGPU(RenderTexture source, RenderTexture target)
        {
#if UNITY_EDITOR
            colorComputeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(shaderPath);
            if (colorComputeShader == null)
            {
                Debug.LogError("ComputeShaderが読み込めませんでした。");
                return;
            }
#endif
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
            colorComputeShader.SetTexture(0, "_balanceModeV3Gradient", GradientToTexture(_balanceModeConfiguration.V3GradientColor));

            // 追加設定
            colorComputeShader.SetBool("_advancedColorModeEnabled", _isAdvancedColorMode);
            colorComputeShader.SetFloat("_advancedColorSettingsBrightness", _advancedColorConfiguration.Brightness);
            colorComputeShader.SetFloat("_advancedColorSettingsContrast", _advancedColorConfiguration.Contrast);
            colorComputeShader.SetFloat("_advancedColorSettingsGamma", _advancedColorConfiguration.Gamma);
            colorComputeShader.SetFloat("_advancedColorSettingsExposure", _advancedColorConfiguration.Exposure);
            colorComputeShader.SetFloat("_advancedColorSettingsTransparency", _advancedColorConfiguration.Transparency);

            int threadGroupX = Mathf.CeilToInt(source.width / 8.0f);
            int threadGroupY = Mathf.CeilToInt(source.height / 8.0f);
            colorComputeShader.Dispatch(kernel, threadGroupX, threadGroupY, 1);
        }

        public Texture2D GradientToTexture(Gradient gradient, int width = 256)
        {
            var texture = new Texture2D(width, 1, TextureFormat.RGBA32, false, true);
            texture.wrapMode = TextureWrapMode.Clamp;

            for (int i = 0; i < width; i++)
            {
                float t = i / (float)(width - 1);
                Color col = gradient.Evaluate(t);
                texture.SetPixel(i, 0, col);
            }

            texture.Apply();
            return texture;
        }
    }
}
