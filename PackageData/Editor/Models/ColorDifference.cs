using net.puk06.ColorChanger.Utils;
using UnityEngine;

namespace net.puk06.ColorChanger.Models
{
    public class ColorDifference
    {
        internal Color PreviousColor { get; private set; }
        internal Color NewColor { get; private set; }

        internal int DiffR { get; private set; }
        internal int DiffG { get; private set; }
        internal int DiffB { get; private set; }

        public ColorDifference(Color previousColor, Color newColor)
        {
            PreviousColor = previousColor;
            NewColor = newColor;

            DiffR = ColorUtils.ConvertColorToInt32(newColor.r) - ColorUtils.ConvertColorToInt32(previousColor.r);
            DiffG = ColorUtils.ConvertColorToInt32(newColor.g) - ColorUtils.ConvertColorToInt32(previousColor.g);
            DiffB = ColorUtils.ConvertColorToInt32(newColor.b) - ColorUtils.ConvertColorToInt32(previousColor.b);
        }
    }
}
