using UnityEngine;
using UnityEditor;
using net.puk06.ColorChanger.Utils;

namespace net.puk06.ColorChanger
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
            if (AssetUtils.Icon == null || EditorUtility.InstanceIDToObject(instanceID) is not GameObject obj) return;

            if (obj.GetComponent<ColorChangerForUnity>() != null)
            {
                Rect iconRect = new Rect(selectionRect.xMax - 16, selectionRect.y, 16, 16);
                GUI.Label(iconRect, AssetUtils.Icon);
            }
        }
    }
}
