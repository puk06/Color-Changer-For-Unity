#nullable enable
using System;
using net.puk06.ColorChanger.Editor.Utils;
using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Models
{
    public class ExtendedRenderTexture : RenderTexture, IDisposable
    {
        private bool _disposed = false;
        private readonly bool _isLinear = true;

        public ExtendedRenderTexture(int width, int height, RenderTextureReadWrite readWrite = RenderTextureReadWrite.sRGB)
            : base(width, height, 0, RenderTextureFormat.ARGB32, readWrite)
        {
            enableRandomWrite = true;
            wrapMode = TextureWrapMode.Clamp;
            filterMode = FilterMode.Bilinear;
            _isLinear = readWrite == RenderTextureReadWrite.Linear;
        }

        public static bool TryCreate(int width, int height, out ExtendedRenderTexture result)
        {
            result = null!;

            ExtendedRenderTexture texture = new(width, height);
            if (!texture.Create())
            {
                texture.Dispose();
                return false;
            }

            result = texture;
            return true;
        }

        public static bool TryCreate(int width, int height, RenderTextureReadWrite readWrite, out ExtendedRenderTexture result)
        {
            result = null!;

            ExtendedRenderTexture texture = new(width, height, readWrite);
            if (!texture.Create())
            {
                texture.Dispose();
                return false;
            }

            result = texture;
            return true;
        }


        public Texture2D ToTexture2D()
        {
            Texture2D texture = new(width, height, TextureFormat.RGBA32, false, _isLinear);
            TextureUtils.ApplyStreamingMipmaps(texture);

            RenderTexture previous = active;
            active = this;

            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();

            active = previous;

            return texture;
        }

        public void Copy(Texture texture) => Graphics.Blit(texture, this);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (active == this) active = null;

            DiscardContents();
            Release();
            DestroyImmediate(this);
        }
    }
}
