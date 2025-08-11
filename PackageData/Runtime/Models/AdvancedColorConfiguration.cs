namespace ColorChanger.Models
{
    public class AdvancedColorConfiguration
    {
        public bool Enabled { get; set; } = false;
        public float Brightness { get; set; } = 1.0f;
        public float Contrast { get; set; } = 1.0f;
        public float Gamma { get; set; } = 1.0f;
        public float Exposure { get; set; } = 0.0f;
        public float Transparency { get; set; } = 0.0f;
    }
}
