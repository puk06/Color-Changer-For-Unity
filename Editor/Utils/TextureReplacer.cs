using UnityEditor;
using UnityEngine;

namespace net.puk06.ColorChanger.Utils
{
    public static class TextureReplacer
    {
        /// <summary>
        /// project内のすべてのマテリアルを検索し、
        /// oldTexを参照しているマテリアルのテクスチャを
        /// newTexPath のテクスチャに置き換える（Undo対応）
        /// </summary>
        public static void ReplaceTextureInMaterials(Texture2D oldTex, string newTexPath)
        {
            if (oldTex == null || string.IsNullOrEmpty(newTexPath))
            {
                Debug.LogError("oldTexがnull、または newTexPath が空です。");
                return;
            }

            // newTexをパスから読み込む
            Texture2D newTex = AssetDatabase.LoadAssetAtPath<Texture2D>(newTexPath);
            if (newTex == null)
            {
                Debug.LogError($"指定されたパスからテクスチャを読み込めませんでした: {newTexPath}");
                return;
            }

            // プロジェクト内のすべてのマテリアルを取得
            string[] materialGuids = AssetDatabase.FindAssets("t:Material");
            int replacedCount = 0;

            foreach (var guid in materialGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

                if (mat == null) continue;
                Undo.RecordObject(mat, "Replace Texture");

                bool replaced = false;

                // マテリアルのすべてのテクスチャプロパティをチェック
                Shader shader = mat.shader;
                int count = ShaderUtil.GetPropertyCount(shader);

                for (int i = 0; i < count; i++)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        string propName = ShaderUtil.GetPropertyName(shader, i);
                        Texture currentTex = mat.GetTexture(propName);

                        if (currentTex == oldTex)
                        {
                            mat.SetTexture(propName, newTex);
                            replaced = true;
                        }
                    }
                }

                if (replaced)
                {
                    replacedCount++;
                    EditorUtility.SetDirty(mat);
                }
            }

            Debug.Log($"{replacedCount} 個のマテリアルのテクスチャを差し替えました。");
        }
    }
}
