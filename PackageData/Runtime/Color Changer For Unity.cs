using UnityEngine;
using net.puk06.ColorChanger.Models;

namespace net.puk06.ColorChanger
{
    public class ColorChangerForUnity : MonoBehaviour
    {
        public Texture2D targetTexture;

        public Color previousColor;
        public Color newColor;
        public BalanceModeConfiguration balanceModeConfiguration = new BalanceModeConfiguration();
        public AdvancedColorConfiguration advancedColorConfiguration = new AdvancedColorConfiguration();
    }
}
