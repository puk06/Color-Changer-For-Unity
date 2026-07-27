using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Extension
{
    internal static class GradientExtensions
    {
        internal static Texture2D ToTexture(this Gradient gradient, int width, int height = 1)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };

            for (int i = 0; i < width; i++)
            {
                var value = i / (float)(width - 1);
                var color = gradient.Evaluate(value);
                texture.SetPixel(i, 0, color);
            }

            texture.Apply();

            return texture;
        }
    }
}
