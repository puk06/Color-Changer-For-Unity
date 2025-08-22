using net.puk06.ColorChanger.Models;
using UnityEditor;
using UnityEngine;

namespace net.puk06.ColorChanger.Utils
{
    [InitializeOnLoad]
    internal static class ExternalUtils
    {
        static ExternalUtils()
            => ColorChangerForUnity.action = ProcessTexture;

        private static void ProcessTexture(RenderTexture renderTexture, ColorChangerForUnity component)
        {
            ExtendedRenderTexture originalTexture = new ExtendedRenderTexture(renderTexture)
                .Create(renderTexture);

            TextureUtils.ProcessTexture(originalTexture, renderTexture, component);

            originalTexture.Dispose();
        }
    }
}
