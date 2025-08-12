using System;

namespace net.puk06.ColorChanger.Models
{
    [Serializable]
    public class AdvancedColorConfiguration
    {
        public bool Enabled = false;
        public float Brightness = 1.0f;
        public float Contrast = 1.0f;
        public float Gamma = 1.0f;
        public float Exposure = 0.0f;
        public float Transparency = 0.0f;
    }
}
