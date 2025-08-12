using System;
using System.Collections.Generic;
using net.puk06.ColorChanger.Models;
using UnityEngine;

namespace net.puk06.ColorChanger.Utils
{
    public static class ColorUtils
    {
        internal static Color32 BalanceColorAdjustment(
            Color32 pixel,
            ColorDifference diff,
            BalanceModeConfiguration balanceModeConfiguration
        )
        {
            double distance = GetColorDistance(pixel, diff.PreviousColor);

            double adjustmentFactor = 0.0;

            switch (balanceModeConfiguration.ModeVersion)
            {
                case 1:
                    var (hasIntersection, intersectionDistance) = GetRGBIntersectionDistance(diff.PreviousColor, pixel);

                    adjustmentFactor = CalculateColorChangeRate(
                        hasIntersection,
                        intersectionDistance,
                        distance,
                        balanceModeConfiguration.V1Weight,
                        balanceModeConfiguration.V1MinimumValue
                    );
                    break;

                case 2:
                    if (distance <= balanceModeConfiguration.V2Radius)
                    {
                        adjustmentFactor = CalculateColorChangeRate(
                            true,
                            balanceModeConfiguration.V2Radius,
                            distance,
                            balanceModeConfiguration.V2Weight,
                            balanceModeConfiguration.V2MinimumValue
                        );
                    }
                    else if (balanceModeConfiguration.V2IncludeOutside)
                    {
                        adjustmentFactor = balanceModeConfiguration.V2MinimumValue;
                    }
                    break;

                case 3:
                    // 赤は0.299、緑は0.587、青は0.114の重みを使ってグレースケール値を計算
                    // この値は、人間の視覚における色の感度を考慮した輝度法に基づいています
                    // コード内で使用しているグレースケールの値に関する詳細はこちらから: https://en.wikipedia.org/wiki/Grayscale
                    const double grayScaleWeightR = 0.299;
                    const double grayScaleWeightG = 0.587;
                    const double grayScaleWeightB = 0.114;

                    float grayScale = (float)(
                        (grayScaleWeightR * (pixel.r / 255.0)) +
                        (grayScaleWeightG * (pixel.g / 255.0)) +
                        (grayScaleWeightB * (pixel.b / 255.0))
                    );

                    return balanceModeConfiguration.V3GradientColor.Evaluate(grayScale);
            }

            pixel.r = MathUtils.ClampColorValue(pixel.r + (int)(diff.DiffR * adjustmentFactor));
            pixel.g = MathUtils.ClampColorValue(pixel.g + (int)(diff.DiffG * adjustmentFactor));
            pixel.b = MathUtils.ClampColorValue(pixel.b + (int)(diff.DiffB * adjustmentFactor));

            return pixel;
        }

        /// <summary>
        /// 2つの色の距離を計算する
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <returns></returns>
        internal static double GetColorDistance(Color32 color1, Color color2)
            => GetColorDistanceInternal((color1.r, color1.b, color1.b), (ConvertColorToInt32(color2.r), ConvertColorToInt32(color2.g), ConvertColorToInt32(color2.b)));

        private static double GetColorDistanceInternal((int R, int G, int B) c1, (int R, int G, int B) c2)
        {
            double r = Math.Pow(c1.R - c2.R, 2);
            double g = Math.Pow(c1.G - c2.G, 2);
            double b = Math.Pow(c1.B - c2.B, 2);

            return Math.Sqrt(r + g + b);
        }

        /// <summary>
        /// 重みから色の変化率を計算する
        /// </summary>
        /// <param name="hasIntersection"></param>
        /// <param name="intersectionDistance"></param>
        /// <param name="distance"></param>
        /// <param name="graphWeight"></param>
        /// <param name="minValue"></param>
        /// <returns></returns>
        internal static double CalculateColorChangeRate(bool hasIntersection, double intersectionDistance, double distance, double graphWeight, double minValue)
        {
            if (!hasIntersection || Math.Abs(intersectionDistance) < MathUtils.EPSILON) return 1;
            double changeRate = Math.Pow(1 - (distance / intersectionDistance), graphWeight);
            return Math.Max(minValue, changeRate);
        }

        /// <summary>
        /// RGB空間の壁に交差する点までの距離を計算する
        /// </summary>
        /// <param name="baseColor"></param>
        /// <param name="targetColor"></param>
        /// <returns></returns>
        internal static (bool hasIntersection, double IntersectionDistance) GetRGBIntersectionDistance(Color baseColor, Color32 targetColor)
        {
            // 基準色
            int base_r = ConvertColorToInt32(baseColor.r);
            int base_g = ConvertColorToInt32(baseColor.g);
            int base_b = ConvertColorToInt32(baseColor.b);

            // 目標色
            int target_r = targetColor.r;
            int target_g = targetColor.g;
            int target_b = targetColor.b;

            // 方向ベクトル
            int dx = target_r - base_r;
            int dy = target_g - base_g;
            int dz = target_b - base_b;

            // 無限線分を RGB 空間の壁に交差する点まで伸ばす（各軸で）
            List<double> t_values = new();

            // 各チャンネルの0と255の壁について、t値（ベクトル方向に進むスカラー量）を求める
            if (dx != 0)
            {
                t_values.Add((0 - base_r) / (double)dx);
                t_values.Add((255 - base_r) / (double)dx);
            }

            if (dy != 0)
            {
                t_values.Add((0 - base_g) / (double)dy);
                t_values.Add((255 - base_g) / (double)dy);
            }

            if (dz != 0)
            {
                t_values.Add((0 - base_b) / (double)dz);
                t_values.Add((255 - base_b) / (double)dz);
            }

            // 最小正の t を探す（延長線上、前方方向）
            double minPositiveT = double.MaxValue;
            foreach (double t in t_values)
            {
                if (t > 0)
                {
                    double x = base_r + (t * dx);
                    double y = base_g + (t * dy);
                    double z = base_b + (t * dz);

                    // 点がRGB空間内にあるか（各成分が0〜255の間）
                    if (
                        x >= 0 && x <= 255 &&
                        y >= 0 && y <= 255 &&
                        z >= 0 && z <= 255 &&
                        t < minPositiveT
                    )
                    {
                        minPositiveT = t;
                    }
                }
            }

            // 最短距離 = ベクトルの長さ * t
            if (Math.Abs(minPositiveT - double.MaxValue) > MathUtils.EPSILON)
            {
                double length = Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
                return (true, minPositiveT * length);
            }

            // 交差しない場合
            return (false, -1);
        }

        /// <summary>
        /// 色の追加設定を行う
        /// </summary>
        /// <param name="pixel"></param>
        /// <param name="advancedColorConfiguration"></param>
        /// <returns></returns>
        internal static Color32 AdvancedColorAdjustment(
            Color32 pixel,
            AdvancedColorConfiguration advancedColorConfiguration
        )
        {
            if (advancedColorConfiguration.Brightness != 1.0f)
                pixel = ApplyBrightness(pixel, advancedColorConfiguration.Brightness);

            if (advancedColorConfiguration.Contrast != 1.0f)
                pixel = ApplyContrast(pixel, advancedColorConfiguration.Contrast);

            if (advancedColorConfiguration.Gamma != 1.0f)
                pixel = ApplyGamma(pixel, advancedColorConfiguration.Gamma);

            if (advancedColorConfiguration.Exposure != 0.0f)
                pixel = ApplyExposure(pixel, advancedColorConfiguration.Exposure);

            if (advancedColorConfiguration.Transparency != 0.0f)
                pixel = ApplyTransparency(pixel, advancedColorConfiguration.Transparency);

            return pixel;
        }

        #region 色の追加設定用メソッド
        private static Color32 ApplyBrightness(Color32 pixel, double brightness)
        {
            pixel.r = MathUtils.ClampColorValue((int)(pixel.r * brightness));
            pixel.g = MathUtils.ClampColorValue((int)(pixel.g * brightness));
            pixel.b = MathUtils.ClampColorValue((int)(pixel.b * brightness));

            return pixel;
        }
        private static Color32 ApplyContrast(Color32 pixel, double contrast)
        {
            pixel.r = MathUtils.ClampColorValue((int)(((pixel.r - 128) * contrast) + 128));
            pixel.g = MathUtils.ClampColorValue((int)(((pixel.g - 128) * contrast) + 128));
            pixel.b = MathUtils.ClampColorValue((int)(((pixel.b - 128) * contrast) + 128));

            return pixel;
        }
        private static Color32 ApplyGamma(Color32 pixel, double gamma)
        {
            pixel.r = MathUtils.ClampColorValue((int)(Math.Pow(pixel.r / 255.0, gamma) * 255));
            pixel.g = MathUtils.ClampColorValue((int)(Math.Pow(pixel.g / 255.0, gamma) * 255));
            pixel.b = MathUtils.ClampColorValue((int)(Math.Pow(pixel.b / 255.0, gamma) * 255));

            return pixel;
        }
        private static Color32 ApplyExposure(Color32 pixel, double exposure)
        {
            pixel.r = MathUtils.ClampColorValue((int)(pixel.r * Math.Pow(2, exposure)));
            pixel.g = MathUtils.ClampColorValue((int)(pixel.g * Math.Pow(2, exposure)));
            pixel.b = MathUtils.ClampColorValue((int)(pixel.b * Math.Pow(2, exposure)));

            return pixel;
        }
        private static Color32 ApplyTransparency(Color32 pixel, double transparency)
        {
            pixel.a = MathUtils.ClampColorValue((int)(pixel.a * (1 - transparency)));

            return pixel;
        }
        #endregion
        
        internal static int ConvertColorToInt32(float colorValue)
        => Mathf.RoundToInt(colorValue * 255f);
    }
}
