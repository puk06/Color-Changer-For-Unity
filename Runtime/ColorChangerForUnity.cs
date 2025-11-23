#nullable enable
using System;
using net.puk06.ColorChanger.Models;
using UnityEngine;

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

        public Texture2D? targetTexture = null;
        public Texture2D?[] settingsInheritedTextures = Array.Empty<Texture2D>();
        public Texture2D? replacementTexture = null;
        
        public Texture2D? maskTexture = null;
        public ImageMaskSelectionType imageMaskSelectionType = ImageMaskSelectionType.None;

        public Color previousColor = Color.white;
        public Color newColor = Color.white;

        public BalanceModeConfiguration balanceModeConfiguration = new BalanceModeConfiguration();
        public AdvancedColorConfiguration advancedColorConfiguration = new AdvancedColorConfiguration();

        /// <summary>
        /// TTTのExternalToolAsLayer用のものです。
        /// </summary>
        public static Action<RenderTexture, ColorChangerForUnity> action = null!;

        public void GrabBlending(RenderTexture renderTexture)
            => action(renderTexture, this);

        /// <summary>
        /// テクスチャ置き換え先があればそのテクスチャを返し、なければTargetTextureが返されます。
        /// </summary>
        public Texture2D? ComponentTexture
        {
            get
            {
                if (targetTexture == null) return null;
                return replacementTexture == null ? targetTexture : replacementTexture;
            }
        }
    }
}
