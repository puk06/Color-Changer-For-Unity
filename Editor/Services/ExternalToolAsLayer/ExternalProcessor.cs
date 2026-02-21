#nullable enable
using net.puk06.ColorChanger.Editor.Models;
using UnityEditor;
using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Services
{
    [InitializeOnLoad]
    internal static class ExternalProcessor
    {
        static ExternalProcessor()
        {
            ColorChangerForUnity.action = ProcessTexture;
        }

        private static void ProcessTexture(RenderTexture targetTexture, ColorChangerForUnity component)
        {
            if (!component.Enabled) return;

            ExtendedRenderTexture originalTexture = new ExtendedRenderTexture(targetTexture, RenderTextureReadWrite.Linear).Create(targetTexture);
            ExtendedRenderTexture? maskTexture = component.MaskTexture != null ? new ExtendedRenderTexture(component.MaskTexture).Create(component.MaskTexture) : null;

            TextureBuilder
                .GetProcessor(component)
                .ProcessRenderTexture(
                    originalTexture,
                    targetTexture,
                    maskTexture,
                    component.ImageMaskSelectionType
                );

            originalTexture.Dispose();
            if (maskTexture != null) maskTexture.Dispose();
        }
    }
}
