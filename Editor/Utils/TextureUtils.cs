#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using net.puk06.ColorChanger.ImageProcessing;
using net.puk06.ColorChanger.Models;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace net.puk06.ColorChanger.Utils
{
    internal static class TextureUtils
    {
        /// <summary>
        /// マテリアルの配列の中で、渡されたテクスチャの画像が入っているMaterialの配列を返します。
        /// </summary>
        /// <param name="materials"></param>
        /// <param name="targetTexture"></param>
        /// <returns></returns>
        internal static List<Material> FindMaterialsWithTexture(Material[] materials, Texture2D targetTexture)
        {
            List<Material> result = new List<Material>();
            if (targetTexture == null) return result;

            foreach (Material material in materials)
            {
                if (material == null) continue;

                var shader = material.shader;
                if (shader == null) continue;

                int count = MaterialUtils.GetPropertyCount(shader);
                if (count == 0) continue;

                for (int i = 0; i < count; i++)
                {
                    if (!MaterialUtils.IsTexture(shader, i)) continue;

                    string propName = ShaderUtil.GetPropertyName(shader, i);
                    if (propName == null) continue;

                    Texture currentTex = material.GetTexture(propName);
                    if (currentTex == null) continue;

                    if (currentTex == targetTexture)
                    {
                        result.Add(material);
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 画像を渡されたコンポーネントの情報を使って処理します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="originalTexture"></param>
        /// <param name="targetTexture"></param>
        /// <param name="maskTexture"></param>
        /// <param name="colorChangerComponent"></param>
        /// <param name="isBuild"></param>
        /// <exception cref="ArgumentException"></exception>
        internal static void ProcessTexture<T>(T originalTexture, T targetTexture, T maskTexture, ColorChangerForUnity colorChangerComponent, bool isBuild = false)
        {
            var colorDifference = new ColorDifference(colorChangerComponent.previousColor, colorChangerComponent.newColor);
            var imageProcessor = new ImageProcessor(colorDifference, isBuild);

            if (colorChangerComponent.balanceModeConfiguration.ModeVersion != 0)
                imageProcessor.SetBalanceSettings(colorChangerComponent.balanceModeConfiguration);

            if (colorChangerComponent.advancedColorConfiguration.Enabled)
                imageProcessor.SetColorSettings(colorChangerComponent.advancedColorConfiguration);

            if (typeof(T) == typeof(Texture2D))
            {
                imageProcessor.ProcessAllPixels(
                    originalTexture as Texture2D,
                    targetTexture as Texture2D,
                    maskTexture as Texture2D,
                    colorChangerComponent.imageMaskSelectionType
                );
            }
            else if (typeof(T) == typeof(ExtendedRenderTexture) || typeof(T) == typeof(RenderTexture))
            {
                imageProcessor.ProcessAllPixelsGPU(
                    originalTexture as RenderTexture,
                    targetTexture as RenderTexture,
                    maskTexture as RenderTexture,
                    colorChangerComponent.imageMaskSelectionType
                );
            }
            else
            {
                throw new ArgumentException($"Unsupported texture type: {typeof(T)}");
            }
        }

        /// <summary>
        /// テクスチャを生成します。
        /// </summary>
        /// <param name="original"></param>
        /// <param name="component"></param>
        /// <param name="useMask"></param>
        /// <returns></returns>
        internal static Texture2D GetProcessedTexture(Texture2D original, ColorChangerForUnity component, bool useMask)
        {
            var gpuResult = TryProcessGPU(original, component, useMask);
            if (gpuResult != null) return gpuResult;

            return TryProcessCPU(original, component, useMask);
        }
        private static Texture2D? TryProcessGPU(Texture2D originalTexture, ColorChangerForUnity component, bool useMask)
        {
            ExtendedRenderTexture? originalRT = null;
            ExtendedRenderTexture? newRT = null;
            ExtendedRenderTexture? maskRT = null;

            try
            {
                originalRT = new ExtendedRenderTexture(originalTexture)
                    .Create(originalTexture);

                newRT = new ExtendedRenderTexture(originalTexture)
                    .Create();

                if (originalRT == null || newRT == null)
                    return null;

                if (useMask && component.maskTexture != null)
                {
                    maskRT = new ExtendedRenderTexture(component.maskTexture)
                        .Create(component.maskTexture);
                }

                ProcessTexture(originalRT, newRT, maskRT, component);

                Texture2D result = GenerateEmptyTexture2D(newRT.width, newRT.height);

                RenderTexture.active = newRT;
                result.ReadPixels(new Rect(0, 0, newRT.width, newRT.height), 0, 0);
                result.Apply();

                return result;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (originalRT != null) originalRT.Dispose();
                if (newRT != null) newRT.Dispose();
                if (maskRT != null) maskRT.Dispose();
            }
        }
        private static Texture2D TryProcessCPU(Texture2D originalTexture, ColorChangerForUnity component, bool useMask)
        {
            Texture2D original2D = GetRawTexture(originalTexture);
            Texture2D result = GenerateEmptyTexture2D(original2D.width, original2D.height);

            Texture2D? mask2D = null;

            try
            {
                if (useMask && component.maskTexture != null)
                    mask2D = GetRawTexture(component.maskTexture);

                ProcessTexture(original2D, result, mask2D, component);

                return result;
            }
            finally
            {
                Object.DestroyImmediate(original2D);
                if (mask2D != null) Object.DestroyImmediate(mask2D);
            }
        }

        /// <summary>
        /// ゲームオブジェクトからメインテクスチャを持ってきます。なければnullを返します。
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        internal static Texture? GetMainTextureFromGameobject(GameObject gameObject)
        {
            if (gameObject == null) return null;

            var renderers = gameObject.GetComponents<Renderer>();
            if (renderers == null || renderers.Length == 0) return null;

            var renderer = renderers.FirstOrDefault();
            if (renderer == null) return null;

            var materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0) return null;

            var mainMaterial = materials.FirstOrDefault();
            if (mainMaterial == null) return null;

            return mainMaterial.mainTexture;
        }

        /// <summary>
        /// レンダラー内の全てのテクスチャのオブジェクトリファレンスを取得します。
        /// </summary>
        /// <param name="renderers"></param>
        /// <returns></returns>
        internal static ObjectReference[] GetRenderersTexturesReferences(IEnumerable<Renderer> renderers)
        {
            return renderers
                .SelectMany(r => r.sharedMaterials)
                .SelectMany(m =>
                {
                    var referenceList = new List<ObjectReference>();
                    MaterialUtils.ForEachTex(m, (texture, _) => referenceList.Add(NDMFUtils.GetReference(texture)));
                    return referenceList;
                })
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// レンダラー内の全てのテクスチャのハッシュセット(比較用)を取得します。
        /// </summary>
        /// <param name="renderers"></param>
        /// <returns></returns>
        internal static HashSet<Texture2D> GetRenderersTexturesHashSet(IEnumerable<Renderer> renderers)
        {
            return renderers
                .SelectMany(r => r.sharedMaterials)
                .SelectMany(m =>
                {
                    var textureList = new List<Texture>();
                    MaterialUtils.ForEachTex(m, (texture, _) => textureList.Add(texture));
                    return textureList;
                })
                .OfType<Texture2D>()
                .Distinct()
                .ToHashSet();
        }

        /// <summary>
        /// アバター内のすべてのレンダラーを取得します。
        /// </summary>
        /// <param name="avatar"></param>
        /// <returns></returns>
        internal static Renderer[] GetRenderers(GameObject avatar)
            => avatar.GetComponentsInChildren<Renderer>(true);

        /// <summary>
        /// Texture2DのRawTextureを取得します。内部でRenderTextureを使います。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="isPreview"></param>
        /// <returns></returns>
        internal static Texture2D GetRawTexture(Texture2D source, bool isPreview = false)
        {
            int width = isPreview ? source.width / 4 : source.width;
            int height = isPreview ? source.height / 4 : source.height;

            Texture2D rawTexture2D = GenerateEmptyTexture2D(width, height);

            ExtendedRenderTexture.ProcessTemporary(width, height, (renderTexture) =>
            {
                Graphics.Blit(source, renderTexture);
                rawTexture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                rawTexture2D.Apply();
            });

            return rawTexture2D;
        }

        internal static Texture2D GenerateEmptyTexture2D(int width, int height)
        {
            Texture2D newTexture = new(width, height, TextureFormat.RGBA32, false, false);

            // どうやらStreaming Mipmapをここで有効にしないといけないらしい
            // 普通に罠過ぎる。

            // This code is borrowed from ReinaS-64892/TexTranstool
            // Github Code URL: https://github.com/ReinaS-64892/TexTransTool/blob/a1f3f1e6e77a066b5fd47f2b692e069cf18b8ff0/Editor/Domain/TextureManager.cs#L192-L200
            // License: MIT License (https://github.com/ReinaS-64892/TexTransTool/blob/master/LICENSE.md)
            SerializedObject textureObject = new(newTexture);
            var sStreamingMipmaps = textureObject.FindProperty("m_StreamingMipmaps");
            sStreamingMipmaps.boolValue = true;

            textureObject.ApplyModifiedPropertiesWithoutUndo();

            return newTexture;
        }
    }
}
