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
            if (!component.Enabled) return;
            
            // 「TTTのRenderTextureはLinear色空間を前提としているため、コピー作成時もLinear色空間としてコピーしないといけない。」というもの。
            // Linear色空間のRenderTextureをsRGB色空間のRenderTextureにコピーしようとした時にLinear -> sRGBへの変換がかかってしまっていたのが原因
            RenderTextureReadWrite readWrite = renderTexture.sRGB ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;
            ExtendedRenderTexture originalTexture = new ExtendedRenderTexture(renderTexture, readWrite)
                .Create(renderTexture);

            TextureUtils.ProcessTexture(originalTexture, renderTexture, component);

            originalTexture.Dispose();
        }
    }
}
