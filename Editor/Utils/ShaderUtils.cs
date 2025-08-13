using UnityEditor;
using UnityEngine;

namespace net.puk06.ColorChanger.Utils
{
    internal static class ShaderUtils
    {
        private const string shaderGuid = "beab1b5262388374abf1a13260545f73";
        private static readonly ComputeShader colorComputeShader;

        static ShaderUtils()
        {
#if UNITY_EDITOR
            string shaderPath = AssetDatabase.GUIDToAssetPath(shaderGuid);
            colorComputeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(shaderPath);
            if (colorComputeShader == null)
            {
                LogUtils.LogError($"ComputeShaderが読み込めませんでした: {shaderPath}");
            }
#endif
        }

        public static ComputeShader GetComputeShader() => colorComputeShader;
    }
}
