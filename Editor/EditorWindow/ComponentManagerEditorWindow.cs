#nullable enable
using System.Collections.Generic;
using System.Linq;
using net.puk06.ColorChanger.Editor.Services;
using net.puk06.ColorChanger.Editor.Utils;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace net.puk06.ColorChanger.Editor
{
    internal class ComponentManagerEditorWindow : EditorWindow
    {
        private sealed class FoldoutState
        {
            public bool Main;
            public bool Enabled;
            public bool Disabled;
        }
        
        private sealed class ComponentStates
        {
            internal List<ColorChangerForUnity> EnabledComponents = new();
            internal List<ColorChangerForUnity> DisabledComponents = new();
        }

        private int _selectedAvatarIndex;
        private readonly Dictionary<Texture, FoldoutState> _foldoutStates = new Dictionary<Texture, FoldoutState>();

        private bool _showMissing = false;

        [MenuItem("Tools/ぷこのつーる/Color Changer For Unity")]
        public static void ShowWindow()
        {
            GetWindow<ComponentManagerEditorWindow>("Color Changer For Unity");
        }

        private void OnGUI()
        {
            LocalizationUtils.DrawLanguageSelectionPopup();

            GameObject[] avatars = FindObjectsOfType<VRC_AvatarDescriptor>().Select(c => c.gameObject).ToArray();
            if (avatars.Length == 0) return;

            _selectedAvatarIndex = Mathf.Clamp(_selectedAvatarIndex, 0, avatars.Length - 1);
            _selectedAvatarIndex = EditorGUILayout.Popup(LocalizationUtils.Localize("EditorWindow.ComponentManager.Avatar"), _selectedAvatarIndex, avatars.Select(a => a.name).ToArray());

            EditorGUILayout.HelpBox(LocalizationUtils.Localize("EditorWindow.ComponentManager.EnabledComponent.Info"), MessageType.Info);

            if (_selectedAvatarIndex >= 0 && _selectedAvatarIndex < avatars.Length && avatars[_selectedAvatarIndex] != null)
            {
                GameObject selectedAvatar = avatars[_selectedAvatarIndex];

                ColorChangerForUnity[] components = selectedAvatar.GetComponentsInChildren<ColorChangerForUnity>(true);
                if (components == null) return;

                Dictionary<Texture2D, ComponentStates> textureComponentDictionary = new();

                foreach (ColorChangerForUnity component in components)
                {
                    void Check(Texture2D texture)
                    {
                        if (!textureComponentDictionary.ContainsKey(texture)) textureComponentDictionary[texture] = new();

                        ComponentStates componentStates = textureComponentDictionary[texture];
                        
                        if (component.gameObject.activeInHierarchy && component.Enabled)
                        {
                            componentStates.EnabledComponents.Add(component);
                        }
                        else
                        {
                            componentStates.DisabledComponents.Add(component);
                        }
                    }

                    if (component.TargetTexture != null) Check(component.TargetTexture);

                    foreach (Texture2D? settingsInheritedTexture in component.SettingsInheritedTextures)
                    {
                        if (settingsInheritedTexture == null) continue;
                        Check(settingsInheritedTexture);
                    }
                }

                foreach (KeyValuePair<Texture2D, ComponentStates> textureComponent in textureComponentDictionary)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUI.indentLevel = 1;
                    if (!_foldoutStates.ContainsKey(textureComponent.Key)) _foldoutStates[textureComponent.Key] = new FoldoutState();

                    int textureCount = textureComponent.Value.EnabledComponents.Count + textureComponent.Value.DisabledComponents.Count;

                    _foldoutStates[textureComponent.Key].Main = EditorGUILayout.Foldout(
                        _foldoutStates[textureComponent.Key].Main,
                        string.Format(LocalizationUtils.Localize("EditorWindow.ComponentManager.Texture"), textureComponent.Key.name, textureCount.ToString()),
                        true,
                        UnityService.TitleStyle
                    );

                    EditorGUI.indentLevel = 2;

                    if (_foldoutStates[textureComponent.Key].Main)
                    {
                        _foldoutStates[textureComponent.Key].Enabled = EditorGUILayout.Foldout(
                            _foldoutStates[textureComponent.Key].Enabled,
                            string.Format(LocalizationUtils.Localize("EditorWindow.ComponentManager.EnabledComponent"), textureComponent.Value.EnabledComponents.Count.ToString()),
                            true,
                            UnityService.SubTitleStyle
                        );

                        if (_foldoutStates[textureComponent.Key].Enabled)
                        {
                            EditorGUI.indentLevel = 3;
                            foreach (ColorChangerForUnity component in textureComponent.Value.EnabledComponents)
                            {
                                EditorGUILayout.ObjectField(component, typeof(ColorChangerForUnity), true);
                            }
                            EditorGUI.indentLevel = 2;
                        }

                        _foldoutStates[textureComponent.Key].Disabled = EditorGUILayout.Foldout(
                            _foldoutStates[textureComponent.Key].Disabled,
                            string.Format(LocalizationUtils.Localize("EditorWindow.ComponentManager.DisabledComponent"), textureComponent.Value.DisabledComponents.Count.ToString()),
                            true,
                            UnityService.SubTitleStyle
                        );

                        if (_foldoutStates[textureComponent.Key].Disabled)
                        {
                            EditorGUI.indentLevel = 3;
                            foreach (ColorChangerForUnity component in textureComponent.Value.DisabledComponents)
                            {
                                EditorGUILayout.ObjectField(component, typeof(ColorChangerForUnity), true);
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
