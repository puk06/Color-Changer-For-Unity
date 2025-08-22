using UnityEngine;
using net.puk06.ColorChanger.Models;
using System;

namespace net.puk06.ColorChanger
{
    [Serializable]
    public class ColorChangerForUnity : MonoBehaviour, VRC.SDKBase.IEditorOnly
#if USE_TEXTRANSTOOL
        , net.rs64.TexTransTool.MultiLayerImage.IExternalToolCanBehaveAsGrabLayerV1
#endif
    {
        public bool Enabled = true;
        public bool PreviewEnabled = true;
        public bool PreviewOnCPU = false;
        public Texture2D targetTexture;

        public Color previousColor = Color.white;
        public Color newColor = Color.white;

        public BalanceModeConfiguration balanceModeConfiguration = new BalanceModeConfiguration();
        public AdvancedColorConfiguration advancedColorConfiguration = new AdvancedColorConfiguration();

        /// <summary>
        /// TTTのExternalToolAsLayer用のものです。
        /// </summary>
        public static Action<RenderTexture, ColorChangerForUnity> action;

        public void GrabBlending(RenderTexture renderTexture)
            => action(renderTexture, this);
    }
}
