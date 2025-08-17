using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace net.puk06.ColorChanger.Utils
{
    internal static class UnityUtils
    {
        private static readonly GUIStyle _titleStyle = new(EditorStyles.foldout)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold
        };

        private static readonly GUIStyle _subTitleStyle = new(EditorStyles.foldout)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold
        };

        /// <summary>
        /// UnityのAssets内で、指定されたパスのファイルを開きます。
        /// </summary>
        /// <param name="assetPath"></param>
        internal static void SelectAssetAtPath(string assetPath)
        {
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (obj != null)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
            else
            {
                LogUtils.LogError("Failed to open the file: " + assetPath);
            }
        }

        /// <summary>
        /// Foldout用のタイトルスタイルです
        /// </summary>
        internal static GUIStyle TitleStyle = _titleStyle;

        /// <summary>
        /// サブのFoldout用のタイトルスタイルです
        /// </summary>
        internal static GUIStyle SubTitleStyle = _subTitleStyle;
    }
}
