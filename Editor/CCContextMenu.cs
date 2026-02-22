using net.puk06.ColorChanger.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace net.puk06.ColorChanger.Editor
{
    public static class CCContextMenu
    {
        private const int Pri = 20;

        private const string MenuBasePath = "GameObject/Color Changer For Unity/"; // Base path for the context menu

        [MenuItem(MenuBasePath + "Add Component with Texture", false, Pri)]
        private static void AddColorChangerToObject() => AddColorChanger(useTextureFromActive: true);

        [MenuItem(MenuBasePath + "Add Empty Component", false, Pri)]
        private static void CreateNewColorChangerObject() => AddColorChanger(useTextureFromActive: false);

        private static void AddColorChanger(bool useTextureFromActive)
        {
            GameObject activeObject = Selection.activeGameObject;

            string objectName = activeObject != null ? $"Color Changer For {activeObject.name}" : "Color Changer For Unity";

            GameObject colorChangerObject = new(objectName);
            if (activeObject != null) colorChangerObject.transform.SetParent(activeObject.transform);

            Undo.RegisterCreatedObjectUndo(colorChangerObject, "Create Color Changer Object");

            ColorChangerForUnity component = Undo.AddComponent<ColorChangerForUnity>(colorChangerObject);
            if (useTextureFromActive && activeObject != null)
            {
                component.TargetTexture = TextureUtils.GetMainTextureFromGameobject(activeObject) as Texture2D;
                if (component.TargetTexture != null) colorChangerObject.name = $"Color Changer For {component.TargetTexture.name}";
            }

            LogUtils.Log($"Component created on '{colorChangerObject.name}'.");
            PingObject(colorChangerObject);
        }

        private static void PingObject(GameObject gameObject)
        {
            if (gameObject == null) return;

            Selection.activeGameObject = gameObject;
            EditorGUIUtility.PingObject(gameObject);
        }
    }
}
