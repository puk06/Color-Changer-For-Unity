using net.puk06.ColorChanger.ImageProcessing;
using net.puk06.ColorChanger.Models;
using System;
using System.Collections.Generic;
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

            foreach (Material material in materials)
            {
                if (material == null) continue;

                var shader = material.shader;
                int count = MaterialUtils.GetPropertyCount(shader);

                for (int i = 0; i < count; i++)
                {
                    if (!MaterialUtils.IsTexture(shader, i)) continue;

                    string propName = ShaderUtil.GetPropertyName(shader, i);
                    Texture currentTex = material.GetTexture(propName);

                    if (currentTex != targetTexture)
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
            else if (typeof(T) == typeof(RenderTexture))
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
        /// Component内のTargetTextureのRenderTextureを取得します。リリースは忘れずに！
        /// </summary>
        /// <param name="colorChanger"></param>
        /// <returns></returns>
        internal static RenderTexture GenerateRenderTexture(ColorChangerForUnity colorChanger)
        {
            var renderTexture = new RenderTexture(colorChanger.targetTexture.width, colorChanger.targetTexture.height, 0);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            return renderTexture;
        }

        /// <summary>
        /// Component内のTargetTextureのRenderTextureを取得します。リリースは忘れずに！
        /// </summary>
        /// <param name="colorChanger"></param>
        /// <returns></returns>
        internal static RenderTexture GenerateRenderTexture(Texture2D targetTexture)
        {
            var renderTexture = new RenderTexture(targetTexture.width, targetTexture.height, 0);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            return renderTexture;
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
                LogUtils.LogError("テクスチャがnullのため読み込まれませんでした。");
                return;
            }

            Texture2D newTex = AssetDatabase.LoadAssetAtPath<Texture2D>(newTexPath);
            if (newTex == null)
            {
                LogUtils.LogError($"指定されたパスからテクスチャを読み込めませんでした: {newTexPath}");
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

            LogUtils.Log($"シーン上の{replacedCount} 個のマテリアルのテクスチャを差し替えました。");
        }
    }
}
