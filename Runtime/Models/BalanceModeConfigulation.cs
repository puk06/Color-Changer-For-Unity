#nullable enable
using System;
using UnityEngine;

namespace net.puk06.ColorChanger.Models
{
    [Serializable]
    public class BalanceModeConfiguration
    {
        public int ModeVersion = 0;

        public float V1Weight = 1.0f;
        public float V1MinimumValue = 0.0f;

        public float V2Weight = 1.0f;
        public float V2Radius = 0.0f;
        public float V2MinimumValue = 0.0f;
        public bool V2IncludeOutside = false;

        public Gradient V3GradientColor = new Gradient();
        public int V3GradientPreviewResolution = 256;
        public int V3GradientBuildResolution = 1024;
    }
}
