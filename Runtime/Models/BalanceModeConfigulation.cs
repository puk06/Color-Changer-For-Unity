#nullable enable
using System;
using UnityEngine;
using UnityEngine.Serialization;

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

        [FormerlySerializedAs("V3GradientColor")]
        public Gradient V3Gradient = new Gradient();

        [FormerlySerializedAs("V3GradientBuildResolution")]
        [Range(2, 2048)] public int V3GradientResolution = 1024;
    }
}
