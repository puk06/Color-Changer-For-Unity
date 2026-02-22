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
            try
            {
                ExtendedRenderTexture originalRenderTexture = new ExtendedRenderTexture(sourceTexture).Create(sourceTexture);
                ExtendedRenderTexture targetRenderTexture = new ExtendedRenderTexture(originalRenderTexture).Create(originalRenderTexture);

                if (originalRenderTexture == null || targetRenderTexture == null)
                {
                    if (originalRenderTexture != null) originalRenderTexture.Dispose();
                    if (targetRenderTexture != null) targetRenderTexture.Dispose();
                    return null;
                }

                ExtendedRenderTexture? maskRenderTexture = (useMask && component.MaskTexture != null) ? new ExtendedRenderTexture(component.MaskTexture).Create(component.MaskTexture) : null;

                GetProcessor(component)
                    .ProcessRenderTexture(
                        originalRenderTexture,
                        targetRenderTexture,
                        maskRenderTexture,
                        component.ImageMaskSelectionType
                    );

                
                originalRenderTexture.Dispose();
                if (maskRenderTexture != null) maskRenderTexture.Dispose();

                return targetRenderTexture;
            }
            catch
            {
                return null;
            }
        }

        internal static ImageProcessor GetProcessor(ColorChangerForUnity colorChangerComponent)
        {
            ImageProcessor imageProcessor = new(colorChangerComponent);

            if (colorChangerComponent.BalanceModeConfiguration.ModeVersion != 0)
                imageProcessor.SetBalanceSettings(colorChangerComponent.BalanceModeConfiguration);

            if (colorChangerComponent.AdvancedColorConfiguration.IsEnabled)
                imageProcessor.SetColorSettings(colorChangerComponent.AdvancedColorConfiguration);

            return imageProcessor;
        }
    }
}
