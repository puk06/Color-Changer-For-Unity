using UnityEditor;
using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Extentions
{
    internal static class ShaderExtensions
    {
        internal static bool IsTexture(this Shader shader, int propertyIndex)
        {
            if (shader == null) return false;
            return ShaderUtil.GetPropertyType(shader, propertyIndex) == ShaderUtil.ShaderPropertyType.TexEnv;
        }
    }
}
