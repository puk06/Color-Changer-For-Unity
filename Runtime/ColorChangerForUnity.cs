using UnityEngine;
using net.puk06.ColorChanger.Models;
using System;

namespace net.puk06.ColorChanger
{
    [Serializable]
    public class ColorChangerForUnity : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        public bool Enabled = true;
        public bool PreviewEnabled = true;
        public bool PreviewOnCPU = false;
        public Texture2D targetTexture;

        public Color previousColor = Color.white;
        public Color newColor = Color.white;

        public BalanceModeConfiguration balanceModeConfiguration = new BalanceModeConfiguration();
        public AdvancedColorConfiguration advancedColorConfiguration = new AdvancedColorConfiguration();
    }
}
