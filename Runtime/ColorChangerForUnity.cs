#nullable enable
using System;
using net.puk06.ColorChanger.Models;
using UnityEngine;
using UnityEngine.Serialization;

namespace net.puk06.ColorChanger
{
    [Serializable]
    public class ColorChangerForUnity : MonoBehaviour, VRC.SDKBase.IEditorOnly, ISerializationCallbackReceiver
#if USE_TEXTRANSTOOL
        , net.rs64.TexTransTool.MultiLayerImage.IExternalToolCanBehaveAsGrabLayerV1
#endif
    {
        private const int CurrentSerializationVersion = 1;
        [SerializeField] private int SerializationVersion = 0;

        [FormerlySerializedAs("Enabled")]
        public bool IsEnabled = true;

        [FormerlySerializedAs("PreviewEnabled")]
        public bool IsPreviewEnabled = true;

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
        public BalanceModeConfiguration BalanceModeConfiguration = new();

        [FormerlySerializedAs("advancedColorConfiguration")]
        public AdvancedColorConfiguration AdvancedColorConfiguration = new();

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

        public void OnBeforeSerialize()
        {
            SerializationVersion = CurrentSerializationVersion;
        }

        public void OnAfterDeserialize()
        {
            if (SerializationVersion >= CurrentSerializationVersion) return;

            if (SerializationVersion == 0)
            {
                if ((int)ImageMaskSelectionType == 6) ImageMaskSelectionType = ImageMaskSelectionType.Black;
                else if ((int)ImageMaskSelectionType == 7) ImageMaskSelectionType = ImageMaskSelectionType.White;
            }

            SerializationVersion = CurrentSerializationVersion;
        }
    }
}
