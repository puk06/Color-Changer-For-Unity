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
        /// セクションヘッダーを作成します
        /// </summary>
        /// <param name="title"></param>
        /// <param name="lineThickness"></param>
        /// <param name="space"></param>
        internal static void DrawSectionHeader(string title, int lineThickness = 2, int space = 4)
        {
            EditorGUILayout.Space(10);

            Rect rect = EditorGUILayout.GetControlRect(false, 20f);

            GUIStyle style = EditorStyles.boldLabel;
            style.fontSize = 15;
            EditorGUI.LabelField(rect, title, style);

            Vector2 titleSize = style.CalcSize(new GUIContent(title));

            float lineX = rect.x + titleSize.x + 8f;
            float lineY = rect.y + rect.height / 2f;

            EditorGUI.DrawRect(
                new Rect(lineX, lineY, rect.width - titleSize.x - 10f, lineThickness),
                new Color(0.3f, 0.3f, 0.3f)
            );

            GUILayout.Space(space);
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
