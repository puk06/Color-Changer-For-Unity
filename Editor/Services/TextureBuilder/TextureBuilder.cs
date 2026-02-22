#nullable enable
using net.puk06.ColorChanger.Editor.Models;
using net.puk06.ColorChanger.Editor.Service;
using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Services
{
    internal static class TextureBuilder
    {
        internal static ExtendedRenderTexture? Build(Texture2D sourceTexture, ColorChangerForUnity component, bool useMask)
        {
            if (!component.gameObject.activeInHierarchy || component.ComponentTexture == null) return null;
            return Process(sourceTexture, component, useMask);
        }

        internal static ExtendedRenderTexture? Process(Texture2D sourceTexture, ColorChangerForUnity component, bool useMask)
        {
            if (!ExtendedRenderTexture.TryCreate(sourceTexture.width, sourceTexture.height, out ExtendedRenderTexture originalRenderTexture))
                return null;
            originalRenderTexture.Copy(sourceTexture);

            using (originalRenderTexture)
            {
                if (!ExtendedRenderTexture.TryCreate(sourceTexture.width, sourceTexture.height, out ExtendedRenderTexture targetRenderTexture))
                    return null;

                ExtendedRenderTexture? maskRenderTexture = null;
                if (useMask && component.MaskTexture != null)
                {
                    if (!ExtendedRenderTexture.TryCreate(component.MaskTexture.width, component.MaskTexture.height, out maskRenderTexture))
                    {
                        targetRenderTexture.Dispose();
                        return null;
                    }

                    maskRenderTexture.Copy(component.MaskTexture);
                }

                using (maskRenderTexture)
                {
                    new ImageProcessor(component)
                        .Process(
                            originalRenderTexture,
                            targetRenderTexture,
                            maskRenderTexture,
                            component.ImageMaskSelectionType
                        );
                }

                return targetRenderTexture;
            }
        }
    }
}
