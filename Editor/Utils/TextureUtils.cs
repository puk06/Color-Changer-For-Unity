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
    }
}
