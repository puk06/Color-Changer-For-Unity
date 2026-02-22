using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Extension
{
    internal static class GradientExtensions
    {
        internal static Texture2D ToTexture(this Gradient gradient, int width, int height = 1)
        {
            Texture2D texture = new(width, height, TextureFormat.RGBA32, false, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };

            for (int i = 0; i < width; i++)
            {
                float value = i / (float)(width - 1);
                Color color = gradient.Evaluate(value);
                texture.SetPixel(i, 0, color);
            }

            texture.Apply();

            return texture;
        }
    }
}
