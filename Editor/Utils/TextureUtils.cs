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
        /// <param name="colorChangerComponent"></param>
        /// <exception cref="ArgumentException"></exception>
        internal static void ProcessTexture<T>(T originalTexture, T targetTexture, ColorChangerForUnity colorChangerComponent)
        {
            var colorDifference = new ColorDifference(colorChangerComponent.previousColor, colorChangerComponent.newColor);
            var imageProcessor = new ImageProcessor(colorDifference);

            if (colorChangerComponent.balanceModeConfiguration.ModeVersion != 0)
                imageProcessor.SetBalanceSettings(colorChangerComponent.balanceModeConfiguration);

            if (colorChangerComponent.advancedColorConfiguration.Enabled)
                imageProcessor.SetColorSettings(colorChangerComponent.advancedColorConfiguration);

            if (typeof(T) == typeof(Texture2D))
            {
                imageProcessor.ProcessAllPixels(
                    originalTexture as Texture2D,
                    targetTexture as Texture2D
                );
            }
            else if (typeof(T) == typeof(ExtendedRenderTexture) || typeof(T) == typeof(RenderTexture))
            {
                imageProcessor.ProcessAllPixelsGPU(
                    originalTexture as RenderTexture,
                    targetTexture as RenderTexture
                );
            }
            else
            {
                throw new ArgumentException($"Unsupported texture type: {typeof(T)}");
            }
        }

        /// <summary>
        /// シーン上のオブジェクトで、渡されたテクスチャを使っているものを全て新しいテクスチャに置き換えます。
        /// </summary>
        /// <param name="oldTex"></param>
        /// <param name="newTexPath"></param>
        internal static void ReplaceTextureInSceneObjects(Texture2D oldTex, string newTexPath)
        {
            if (oldTex == null || string.IsNullOrEmpty(newTexPath))
            {
                LogUtils.LogError("Failed to load texture because it was null.");
                return;
            }

            Texture2D newTex = AssetDatabase.LoadAssetAtPath<Texture2D>(newTexPath);
            if (newTex == null)
            {
                LogUtils.LogError($"Failed to load texture from the specified path: {newTexPath}");
                return;
            }

            Renderer[] renderers = Object.FindObjectsOfType<Renderer>();
            int replacedCount = 0;

            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat == null) continue;

                    Undo.RecordObject(mat, "Replace Texture");
                    bool replaced = false;

                    MaterialUtils.ForEachTex(mat, (texture, propName) =>
                    {
                        if (texture != oldTex) return;
                        mat.SetTexture(propName, newTex);
                        replaced = true;
                    });

                    if (replaced)
                    {
                        replacedCount++;
                        EditorUtility.SetDirty(mat);
                    }
                }
            }

            LogUtils.Log($"Replaced textures for {replacedCount} materials in the scene.");
        }

        /// <summary>
        /// ゲームオブジェクトからメインテクスチャを持ってきます。なければnullを返します。
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        internal static Texture GetMainTextureFromGameobject(GameObject gameObject)
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
        /// <returns></returns>
        internal static Texture2D GetRawTexture(Texture2D source)
        {
            Texture2D rawTexture2D = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);

            ExtendedRenderTexture.ProcessTemporary(source.width, source.height, (renderTexture) =>
            {
                Graphics.Blit(source, renderTexture);
                rawTexture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                rawTexture2D.Apply();
            });

            return rawTexture2D;
        }
    }
}
