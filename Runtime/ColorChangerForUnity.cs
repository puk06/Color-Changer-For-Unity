#nullable enable
using System;
using System.Linq;
using net.puk06.ColorChanger.Models;
using UnityEngine;
using UnityEngine.Serialization;

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

        [FormerlySerializedAs("targetTexture")]
        public Texture2D? TargetTexture = null;

        [FormerlySerializedAs("settingsInheritedTextures")]
        public Texture2D?[] SettingsInheritedTextures = Array.Empty<Texture2D>();

        [FormerlySerializedAs("replacementTexture")]
        public Texture2D? ReplacementTexture = null;
        
        [FormerlySerializedAs("maskTexture")]
        public Texture2D? MaskTexture = null;

        [FormerlySerializedAs("imageMaskSelectionType")]
        public ImageMaskSelectionType ImageMaskSelectionType = ImageMaskSelectionType.None;

        [FormerlySerializedAs("previousColor")]
        public Color SourceColor = Color.white;

        [FormerlySerializedAs("newColor")]
        public Color TargetColor = Color.white;

        [FormerlySerializedAs("balanceModeConfiguration")]
        public BalanceModeConfiguration BalanceModeConfiguration = new BalanceModeConfiguration();

        [FormerlySerializedAs("advancedColorConfiguration")]
        public AdvancedColorConfiguration AdvancedColorConfiguration = new AdvancedColorConfiguration();

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
                if (TargetTexture == null) return null;
                return ReplacementTexture == null ? TargetTexture : ReplacementTexture;
            }
        }

        public Texture2D[] SafeSettingsInheritedTextures
            => SettingsInheritedTextures.Where(t => t != null).ToArray()!;
    }
}
