using net.puk06.ColorChanger.ImageProcessing;
using net.puk06.ColorChanger.Models;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace net.puk06.ColorChanger.Utils
{
    internal static class TextureUtils
    {
        internal static List<Material> FindMaterialsWithTexture(Material[] materials, Texture2D targetTexture)
        {
            List<Material> result = new List<Material>();

            foreach (Material material in materials)
            {
                if (material == null) continue;
                var shader = material.shader;
                int count = ShaderUtil.GetPropertyCount(shader);

                for (int i = 0; i < count; i++)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) != ShaderUtil.ShaderPropertyType.TexEnv)
                        continue;

                    string propName = ShaderUtil.GetPropertyName(shader, i);
                    Texture currentTex = material.GetTexture(propName);

                    if (currentTex == targetTexture)
                    {
                        result.Add(material);
                        break;
                    }
                }
            }

            return result;
        }

        internal static void ProcessTexture(Texture2D originalTexture, Texture2D targetTexture, ColorChangerForUnity colorChangerComponent)
        {
            ColorDifference colorDifference = new ColorDifference(colorChangerComponent.previousColor, colorChangerComponent.newColor);

            ImageProcessor imageProcessor = new ImageProcessor(colorDifference);

            if (colorChangerComponent.balanceModeConfiguration.ModeVersion != 0)
                imageProcessor.SetBalanceSettings(colorChangerComponent.balanceModeConfiguration);

            if (colorChangerComponent.advancedColorConfiguration.Enabled)
                imageProcessor.SetColorSettings(colorChangerComponent.advancedColorConfiguration);

            imageProcessor.ProcessAllPixels(originalTexture, targetTexture);
        }

        internal static void ProcessTexture(RenderTexture originalTexture, RenderTexture targetTexture, ColorChangerForUnity colorChangerComponent)
        {
            ColorDifference colorDifference = new ColorDifference(colorChangerComponent.previousColor, colorChangerComponent.newColor);

            ImageProcessor imageProcessor = new ImageProcessor(colorDifference);

            if (colorChangerComponent.balanceModeConfiguration.ModeVersion != 0)
                imageProcessor.SetBalanceSettings(colorChangerComponent.balanceModeConfiguration);

            if (colorChangerComponent.advancedColorConfiguration.Enabled)
                imageProcessor.SetColorSettings(colorChangerComponent.advancedColorConfiguration);

            imageProcessor.ProcessAllPixelsGPU(originalTexture, targetTexture);
        }

        internal static RenderTexture GenerateRenderTexture(ColorChangerForUnity colorChanger)
        {
            var renderTexture = new RenderTexture(colorChanger.targetTexture.width, colorChanger.targetTexture.height, 0);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            return renderTexture;
        }

        internal static void ReplaceTextureInMaterials(Texture2D oldTex, string newTexPath)
        {
            if (oldTex == null || string.IsNullOrEmpty(newTexPath))
            {
                LogUtils.LogError("oldTexがnull、または newTexPath が空です。");
                return;
            }

            Texture2D newTex = AssetDatabase.LoadAssetAtPath<Texture2D>(newTexPath);
            if (newTex == null)
            {
                LogUtils.LogError($"指定されたパスからテクスチャを読み込めませんでした: {newTexPath}");
                return;
            }

            string[] materialGuids = AssetDatabase.FindAssets("t:Material");
            int replacedCount = 0;

            foreach (var guid in materialGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

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

            LogUtils.Log($"{replacedCount} 個のマテリアルのテクスチャを差し替えました。");
        }
    }
}
