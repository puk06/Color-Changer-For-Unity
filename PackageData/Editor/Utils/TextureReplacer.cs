using UnityEditor;
using UnityEngine;

namespace ColorChanger.Utils
{
    public static class TextureReplacer
    {
        /// <summary>
        /// project内のすべてのマテリアルを検索し、
        /// oldTexを参照しているマテリアルのテクスチャをnewTexに置き換える
        /// Undo対応付き
        /// </summary>
        public static void ReplaceTextureInMaterials(Texture2D oldTex, Texture2D newTex)
        {
            if (oldTex == null || newTex == null)
            {
                Debug.LogError("oldTex または newTex が null です");
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

                // Undo登録
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
