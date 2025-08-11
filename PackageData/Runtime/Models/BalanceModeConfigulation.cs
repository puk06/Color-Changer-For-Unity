using UnityEngine;

namespace ColorChanger.Models
{
    public class BalanceModeConfiguration
    {
        public int ModeVersion { get; set; } = 0;

        public float V1Weight { get; set; } = 1.0f;
        public float V1MinimumValue { get; set; } = 0.0f;

        public float V2Weight { get; set; } = 1.0f;
        public float V2Radius { get; set; } = 0.0f;
        public float V2MinimumValue { get; set; } = 0.0f;
        public bool V2IncludeOutside { get; set; } = false;

        public Gradient V3GradientColor { get; set; } = new Gradient();
    }
}
