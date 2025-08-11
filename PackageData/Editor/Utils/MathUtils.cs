using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColorChanger.Utils
{
    public static class MathUtils
    {
        /// <summary>
        /// 浮動小数点の比較に使用する許容誤差
        /// </summary>
        internal const double EPSILON = 1e-6;

        /// <summary>
        /// 数値を0〜255にクランプする
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static byte ClampColorValue(int value)
            => (byte)Math.Clamp(value, 0, 255);
    }
}
