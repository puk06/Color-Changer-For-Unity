#nullable enable
using System.Linq;
using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Utils
{
    internal static class TextureUtils
    {
        /// <summary>
        /// ゲームオブジェクトからメインテクスチャを持ってきます。なければnullを返します。
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        internal static Texture? GetMainTextureFromGameobject(GameObject gameObject)
        {
            if (gameObject == null) return null;

            Renderer[] renderers = gameObject.GetComponents<Renderer>();
            if (renderers == null || renderers.Length == 0) return null;

            Renderer renderer = renderers.FirstOrDefault();
            if (renderer == null) return null;

            Material[] materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0) return null;

            Material mainMaterial = materials.FirstOrDefault();
            if (mainMaterial == null) return null;

            return mainMaterial.mainTexture;
        }

        internal static void ApplyStreamingMipmaps(Texture2D texture)
        {
            UnityEditor.SerializedObject textureObject = new(texture);
            UnityEditor.SerializedProperty streamingMipmapsProperty = textureObject.FindProperty("m_StreamingMipmaps");
            if (streamingMipmapsProperty != null) streamingMipmapsProperty.boolValue = true;
            textureObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
