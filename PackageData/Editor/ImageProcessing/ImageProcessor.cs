using net.puk06.ColorChanger.Models;
using net.puk06.ColorChanger.Utils;
using Unity.Collections;
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
            {
                if (_isAdvancedColorMode)
                        source = ColorUtils.AdvancedColorAdjustment(source, _advancedColorConfiguration);
            }

            return source;
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
    }
}
