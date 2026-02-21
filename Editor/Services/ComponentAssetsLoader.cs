using UnityEditor;
using UnityEngine;

namespace net.puk06.ColorChanger.Utils
{
    internal static class ComponentAssetsLoader
    {
        internal static readonly Texture2D Logo;
        internal static readonly Texture2D Icon;
        
        static ComponentAssetsLoader()
        {
            Logo = AssetDatabase.LoadAssetAtPath<Texture2D>(GetAssetPath("ComponentLogo.png"));
            Icon = AssetDatabase.LoadAssetAtPath<Texture2D>(GetAssetPath("ComponentIcon.png"));
        }

        private const string BASE_ASSET_PATH = "Packages/net.puk06.color-changer/Editor/Assets/";
        private static string GetAssetPath(string fileName) => BASE_ASSET_PATH + fileName;
    }
}
