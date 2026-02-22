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

            if (!ExtendedRenderTexture.TryCreate(targetTexture.width, targetTexture.height, RenderTextureReadWrite.Linear, out ExtendedRenderTexture originalTexture))
                return;
            originalTexture.Copy(targetTexture);

            using (originalTexture)
            {
                ExtendedRenderTexture? maskTexture = null;
                if (component.MaskTexture != null)
                {
                    if (!ExtendedRenderTexture.TryCreate(component.MaskTexture.width, component.MaskTexture.height, out maskTexture))
                        return;
                    maskTexture.Copy(component.MaskTexture);
                }

                using (maskTexture)
                {
                    ExtendedRenderTexture? processedTexture = TextureBuilder.Process(component, originalTexture, maskTexture, component.ImageMaskSelectionType, RenderTextureReadWrite.Linear);
                    if (processedTexture == null) return;

                    Graphics.Blit(processedTexture, targetTexture);
                    processedTexture.Dispose();
                }
            }
        }
    }
}
