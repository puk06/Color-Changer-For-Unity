using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Extension
{
    internal static class ComponentExtensions
    {
        public static bool IsActiveCCComponent(this Component component, bool isPreview = false)
        {
            if (!IsActiveComponent(component)) return false;

            if (component is ColorChangerForUnity colorChangerComponent)
            {
                return colorChangerComponent.IsEnabled && (isPreview == false || colorChangerComponent.IsPreviewEnabled);
            }

            return false;
        }

        public static bool IsActiveComponent(this Component component)
        {
            return component.gameObject.activeInHierarchy && component.IsEditorOnly() == false;
        }

        public static bool IsEditorOnly(this Component component)
        {
            return component.CompareTag("EditorOnly");
        }
    }
}
