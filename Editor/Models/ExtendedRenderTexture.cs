using System;
using net.puk06.ColorChanger.Utils;
using UnityEngine;

namespace net.puk06.ColorChanger.Models
{
    /// <summary>
    /// RenderTextureの継承クラスです。Color Changer用に作られてます。
    /// </summary>
    public class ExtendedRenderTexture : RenderTexture, IDisposable
    {
        private bool _isCreated = false;
        private bool _disposed = false;

        /// <summary>
        /// 渡されたサイズの大きさのExtendedRenderTextureをInitializeします。Createはこの時点では実行されません。
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="readWrite"></param>
        public ExtendedRenderTexture(int width, int height, RenderTextureReadWrite readWrite = RenderTextureReadWrite.sRGB)
            : base(width, height, 0, RenderTextureFormat.ARGB32, readWrite)
        {
            enableRandomWrite = true;
            wrapMode = TextureWrapMode.Clamp;
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
        public ExtendedRenderTexture Create(Texture texture = null)
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
                LogUtils.LogError($"Failed to create RenderTexture. This may be due to the platform not supporting GPU-based computation.");
                Dispose();
            }

            return this;
        }

        /// <summary>
        /// 一時的にRenderTextureでしたい処理をaction内に書くことができます。
        /// 処理中に作成された内部のRenderTextureは自動で開放されます。
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="action"></param>
        public static void ProcessTemporary(int width, int height, Action<RenderTexture> action)
        {
            RenderTexture temporaryRenderTexture = GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);

            RenderTexture previousActiveRenderTexture = active;
            active = temporaryRenderTexture;

            action(temporaryRenderTexture);

            active = previousActiveRenderTexture;
            ReleaseTemporary(temporaryRenderTexture);
        }

        /// <summary>
        /// ExtendedRenderTextureをDisposeします。
        /// </summary>
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
