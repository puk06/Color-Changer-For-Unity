using UnityEditor;
using UnityEngine;

namespace net.puk06.ColorChanger.Utils
{
    internal static class AssetUtils
    {
        internal static readonly Texture2D Logo;
        internal static readonly Texture2D Icon;

        static AssetUtils()
        {
            Logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/net.puk06.color-changer/Editor/Assets/ComponentLogo.png");
            Icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/net.puk06.color-changer/Editor/Assets/ComponentIcon.png");
        }
    }
}
