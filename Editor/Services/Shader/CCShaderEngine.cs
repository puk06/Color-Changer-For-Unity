#nullable enable
using UnityEditor;
using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Services
{
    internal static class CCShaderEngine
    {
        private const string TextureProcessorGuid = "beab1b5262388374abf1a13260545f73";

        internal static ComputeShader? TextureProcessorComputeShader { get; private set; }

        static CCShaderEngine()
        {
            TextureProcessorComputeShader = Load(TextureProcessorGuid);
        }

        private static ComputeShader? Load(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<ComputeShader>(path);
        }
    }
}
