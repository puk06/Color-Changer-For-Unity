using System.Collections.Generic;
using System.Linq;
using net.puk06.ColorChanger.Editor.Services;
using net.puk06.ColorChanger.Models;
using net.puk06.ColorChanger.Utils;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace net.puk06.ColorChanger.Editor
{
    public class FoldoutState
    {
        public bool Main;
        public bool Enabled;
        public bool Disabled;
    }

    public class ColorChangerEditorWindow : EditorWindow
    {
        private int _selectedAvatarIndex;
        private readonly Dictionary<Texture, FoldoutState> _foldoutStates = new Dictionary<Texture, FoldoutState>();

        private bool _showMissing = false;

        [MenuItem("Tools/ぷこのつーる/Color Changer For Unity")]
        public static void ShowWindow()
        {
            GetWindow<ColorChangerEditorWindow>("Color Changer For Unity");
        }

        private void OnGUI()
        {
            LocalizationUtils.DrawLanguageSelectionPopup();

            GameObject[] avatars = FindObjectsOfType<VRC_AvatarDescriptor>().Select(c => c.gameObject).ToArray();
            if (avatars.Length == 0) return;

            _selectedAvatarIndex = Mathf.Clamp(_selectedAvatarIndex, 0, avatars.Length - 1);
            _selectedAvatarIndex = EditorGUILayout.Popup(LocalizationUtils.Localize("EditorWindow.ComponentManager.Avatar"), _selectedAvatarIndex, avatars.Select(a => a.name).ToArray());

            if (_selectedAvatarIndex >= 0 && _selectedAvatarIndex < avatars.Length && avatars[_selectedAvatarIndex] != null)
            {
                GameObject selectedAvatar = avatars[_selectedAvatarIndex];

                ColorChangerForUnity[] components = selectedAvatar.GetComponentsInChildren<ColorChangerForUnity>(true);
                if (components == null) return;

                List<InternalColorChangerValues> internalComponentsValues = new();
                foreach (ColorChangerForUnity component in components)
                {
                    internalComponentsValues.Add(new InternalColorChangerValues(component, component.TargetTexture, component.ComponentTexture, true));
                    foreach (Texture2D otherTexture in component.SettingsInheritedTextures.Where(t => t != null))
                    {
                        internalComponentsValues.Add(new InternalColorChangerValues(component, otherTexture, otherTexture, false));
                    }
                }

                IEnumerable<IGrouping<Texture2D, InternalColorChangerValues>> groupedComponents = internalComponentsValues
                    .Where(c => c.originalTexture != null)
                    .GroupBy(c => c.originalTexture);

                foreach (IGrouping<Texture2D, InternalColorChangerValues> groupedComponent in groupedComponents)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUI.indentLevel = 1;
                    if (!_foldoutStates.ContainsKey(groupedComponent.Key))
                        _foldoutStates[groupedComponent.Key] = new FoldoutState();

                    _foldoutStates[groupedComponent.Key].Main = EditorGUILayout.Foldout(
                        _foldoutStates[groupedComponent.Key].Main,
                        string.Format(LocalizationUtils.Localize("EditorWindow.ComponentManager.Texture"), groupedComponent.Key.name, groupedComponent.Count().ToString()),
                        true,
                        UnityService.TitleStyle
                    );

                    EditorGUI.indentLevel = 2;

                    if (_foldoutStates[groupedComponent.Key].Main)
                    {
                        IEnumerable<InternalColorChangerValues> enabledComponents = groupedComponent.Where(x => x.parentComponent.gameObject.activeSelf && x.parentComponent.Enabled);
                        IEnumerable<InternalColorChangerValues> disabledComponents = groupedComponent.Except(enabledComponents);

                        _foldoutStates[groupedComponent.Key].Enabled = EditorGUILayout.Foldout(
                            _foldoutStates[groupedComponent.Key].Enabled,
                            string.Format(LocalizationUtils.Localize("EditorWindow.ComponentManager.EnabledComponent"), enabledComponents.Count().ToString()),
                            true,
                            UnityService.SubTitleStyle
                        );

                        if (_foldoutStates[groupedComponent.Key].Enabled)
                        {
                            EditorGUI.indentLevel = 3;
                            foreach (var component in enabledComponents)
                            {
                                EditorGUILayout.ObjectField(component.parentComponent, typeof(ColorChangerForUnity), true);
                            }
                            EditorGUI.indentLevel = 2;
                        }

                        _foldoutStates[groupedComponent.Key].Disabled = EditorGUILayout.Foldout(
                            _foldoutStates[groupedComponent.Key].Disabled,
                            string.Format(LocalizationUtils.Localize("EditorWindow.ComponentManager.DisabledComponent"), disabledComponents.Count().ToString()),
                            true,
                            UnityService.SubTitleStyle
                        );

                        if (_foldoutStates[groupedComponent.Key].Disabled)
                        {
                            EditorGUI.indentLevel = 3;
                            foreach (var component in disabledComponents)
                            {
                                EditorGUILayout.ObjectField(component.parentComponent, typeof(ColorChangerForUnity), true);
                            }
                            EditorGUI.indentLevel = 2;
                        }
                    }

                    EditorGUI.indentLevel = 1;

                    EditorGUILayout.EndVertical();
                }

                List<ColorChangerForUnity> missingTextureComponents = components
                    .Where(c => c.TargetTexture == null)
                    .ToList();

                if (missingTextureComponents.Count > 0)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUI.indentLevel = 1;

                    _showMissing = EditorGUILayout.Foldout(
                        _showMissing,
                        string.Format(LocalizationUtils.Localize("EditorWindow.ComponentManager.Texture"), LocalizationUtils.Localize("EditorWindow.ComponentManager.MissingTexture"), missingTextureComponents.Count.ToString()),
                        true,
                        UnityService.TitleStyle
                    );

                    EditorGUI.indentLevel = 2;

                    if (_showMissing)
                    {
                        EditorGUI.indentLevel = 3;
                        foreach (ColorChangerForUnity component in missingTextureComponents)
                        {
                            EditorGUILayout.ObjectField(component, typeof(ColorChangerForUnity), true);
                        }
                        EditorGUI.indentLevel = 2;
                    }

                    EditorGUI.indentLevel = 1;

                    EditorGUILayout.EndVertical();
                }
            }
        }
    }
}
