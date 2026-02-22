#nullable enable
using net.puk06.ColorChanger.Editor.Models;
using net.puk06.ColorChanger.Editor.Service;
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
                    new ImageProcessor(component)
                        .Process(
                            originalTexture,
                            targetTexture,
                            maskTexture,
                            component.ImageMaskSelectionType
                        );
                }
            }
        }
    }
}
