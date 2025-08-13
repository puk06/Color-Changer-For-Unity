using System;
using UnityEditor;
using UnityEngine;

internal static class MaterialUtils
{
    internal static void ForEachTex(Material material, Action<Texture, string> action)
    {
        Shader shader = material.shader;
        int count = ShaderUtil.GetPropertyCount(shader);

        for (int i = 0; i < count; i++)
        {
            if (ShaderUtil.GetPropertyType(shader, i) != ShaderUtil.ShaderPropertyType.TexEnv)
                continue;

            string propName = ShaderUtil.GetPropertyName(shader, i);
            Texture currentTex = material.GetTexture(propName);
            if (currentTex == null) continue;

            action(currentTex as Texture2D, propName);
        }
    }

    internal static bool AnyTex(Material material, Texture targetTexture)
    {
        Shader shader = material.shader;
        int count = ShaderUtil.GetPropertyCount(shader);

        for (int i = 0; i < count; i++)
        {
            if (ShaderUtil.GetPropertyType(shader, i) != ShaderUtil.ShaderPropertyType.TexEnv)
                continue;

            string propName = ShaderUtil.GetPropertyName(shader, i);
            Texture currentTex = material.GetTexture(propName);
            if (currentTex == null) continue;

            if (currentTex == targetTexture) return true;
        }

        return false;
    }
}