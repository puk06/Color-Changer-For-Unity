using net.puk06.ColorChanger.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace net.puk06.ColorChanger.Editor
{
    [InitializeOnLoad]
    public static class ColorChangerIconDrawer
    {
        static ColorChangerIconDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            if (ComponentAssetsLoader.Icon == null || EditorUtility.InstanceIDToObject(instanceID) is not GameObject obj) return;

            if (obj.GetComponent<ColorChangerForUnity>() != null)
            {
                Rect iconRect = new Rect(selectionRect.xMax - 16, selectionRect.y, 16, 16);
                GUI.Label(iconRect, ComponentAssetsLoader.Icon);
            }
        }
    }
}
