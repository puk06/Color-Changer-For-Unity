using System;
using UnityEditor;
using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Extentions
{
    internal static class MaterialExtensions
    {
        internal static bool HasTexture(this Material material, Texture texture)
        {
            bool found = false;
            ForEachTexture(material, (tex, _) =>
            {
                if (Equals(tex, texture)) found = true;
            });
            return found;
        }

        internal static void ForEachTexture(this Material material, Action<Texture, string> action)
        {
            if (material == null || action == null) return;

            Shader shader = material.shader;
            if (shader == null) return;

            int propertyCount = ShaderUtil.GetPropertyCount(shader);
            for (int i = 0; i < propertyCount; i++)
            {
                if (!shader.IsTexture(i)) continue;

                string propName = ShaderUtil.GetPropertyName(shader, i);
                if (propName == null) continue;

                Texture materialTexture = material.GetTexture(propName);
                if (materialTexture == null) continue;

                action(materialTexture, propName);
            }
        }
    }
}
