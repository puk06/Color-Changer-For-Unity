#nullable enable
using System;
using net.puk06.ColorChanger.Editor.Utils;
using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Models
{
    public class ExtendedRenderTexture : RenderTexture, IDisposable
    {
        private bool _isCreated = false;
        private bool _disposed = false;
        public bool Created => _isCreated;

        /// <summary>
        /// 渡されたサイズの大きさのExtendedRenderTextureをInitializeします。Createはこの時点では実行されません。
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="readWrite"></param>
        public ExtendedRenderTexture(int width, int height, RenderTextureReadWrite readWrite = RenderTextureReadWrite.sRGB)
            : base(width, height : height, 0, RenderTextureFormat.ARGB32, readWrite)
        {
            enableRandomWrite = true;
            wrapMode = TextureWrapMode.Clamp;
            filterMode = FilterMode.Bilinear;
        }

        /// <summary>
        /// Texture のサイズの大きさの ExtendedRenderTexture を Initialize します。Createはこの時点では実行されません。
        /// 作成時、中身はコピーされません。中身をコピーしたい場合は <see cref="Create(Texture)"/> を使ってください。
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="readWrite"></param>
        public ExtendedRenderTexture(Texture texture, RenderTextureReadWrite readWrite = RenderTextureReadWrite.sRGB)
            : this(texture.width, texture.height, readWrite)
        {
        }

        /// <summary>
        /// RenderTextureを内部で作成します。すでにCreateが実行されていた際は例外を吐きます。
        /// Textureを渡すことで、作成時に自動的にコピーされます。
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ExtendedRenderTexture Create(Texture? texture = null)
        {
            if (_isCreated) throw new Exception("RenderTexture has already created.");
            if (_disposed) throw new Exception("RenderTexture has already disposed.");

            if (base.Create())
            {
                _isCreated = true;
                if (texture != null) Graphics.Blit(texture, this);
            }
            else
            {
                Debug.LogError($"Failed to create RenderTexture. This may be due to the platform not supporting GPU-based computation.");
            }

            return this;
        }

        public Texture2D ToTexture2D()
        {
            Texture2D texture = new(width, height, TextureFormat.RGBA32, false, false);
            TextureUtils.ApplyStreamingMipmaps(texture);

            RenderTexture previous = active;
            active = this;

            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();

            active = previous;

            return texture;
        }

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
