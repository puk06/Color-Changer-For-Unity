using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace net.puk06.ColorChanger.Models
{
    [Serializable]
    public class AdvancedColorConfiguration
    {
        [FormerlySerializedAs("Enabled")]
        public bool IsEnabled = false;
        
        [Range(0f, 360f)] public float Hue;
        [Range(0f, 100.0f)] public float Saturation;
        [Range(0f, 100.0f)] public float Value;
        public float Brightness = 1.0f;
        public float Contrast = 1.0f;
        public float Gamma = 1.0f;
        public float Exposure = 0.0f;
        [Range(0f, 1.0f)] public float Transparency = 0.0f;
    }
}
