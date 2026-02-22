using net.puk06.ColorChanger.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace net.puk06.ColorChanger.Editor.Services
{
    internal static class UnityService
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

        internal static GUIStyle TitleStyle = _titleStyle;
        internal static GUIStyle SubTitleStyle = _subTitleStyle;
    }
}
