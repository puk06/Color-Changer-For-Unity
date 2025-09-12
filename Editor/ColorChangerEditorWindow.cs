using System.Collections.Generic;
using System.Linq;
using net.puk06.ColorChanger.Localization;
using net.puk06.ColorChanger.Utils;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace net.puk06.ColorChanger
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
            LocalizationUtils.GenerateLanguagePopup();

            //アバターを選ぶ欄
            var avatars = FindObjectsOfType<VRC_AvatarDescriptor>().Select(c => c.gameObject).ToArray();
            if (avatars.Length == 0) return;

            _selectedAvatarIndex = Mathf.Clamp(_selectedAvatarIndex, 0, avatars.Length - 1);
            _selectedAvatarIndex = EditorGUILayout.Popup(LocalizationManager.Get("editorwindow.componentmanager.avatar"), _selectedAvatarIndex, avatars.Select(a => a.name).ToArray());

            if (_selectedAvatarIndex >= 0 && _selectedAvatarIndex < avatars.Length && avatars[_selectedAvatarIndex] != null)
            {
                var selectedAvatar = avatars[_selectedAvatarIndex];

                var components = selectedAvatar.GetComponentsInChildren<ColorChangerForUnity>(true);
                if (components == null) return;

                var groupedComponents = components
                    .Where(c => c.targetTexture != null)
                    .GroupBy(c => c.targetTexture);

                foreach (var groupedComponent in groupedComponents)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUI.indentLevel = 1;
                    if (!_foldoutStates.ContainsKey(groupedComponent.Key))
                        _foldoutStates[groupedComponent.Key] = new FoldoutState();

                    _foldoutStates[groupedComponent.Key].Main = EditorGUILayout.Foldout(
                        _foldoutStates[groupedComponent.Key].Main,
                        LocalizationManager.Get("editorwindow.componentmanager.texture", groupedComponent.Key.name, groupedComponent.Count().ToString()),
                        true,
                        UnityUtils.TitleStyle
                    );

                    EditorGUI.indentLevel = 2;

                    if (_foldoutStates[groupedComponent.Key].Main)
                    {
                        var enabledComponents = groupedComponent.Where(x => ColorChangerUtils.IsEnabled(x));
                        var disabledComponents = groupedComponent.Except(enabledComponents);

                        _foldoutStates[groupedComponent.Key].Enabled = EditorGUILayout.Foldout(
                            _foldoutStates[groupedComponent.Key].Enabled,
                            LocalizationManager.Get("editorwindow.componentmanager.enabledcomponents", enabledComponents.Count().ToString()),
                            true,
                            UnityUtils.SubTitleStyle
                        );

                        if (_foldoutStates[groupedComponent.Key].Enabled)
                        {
                            EditorGUI.indentLevel = 3;
                            foreach (var component in enabledComponents)
                            {
                                EditorGUILayout.ObjectField(component, typeof(ColorChangerForUnity), true);
                            }
                            EditorGUI.indentLevel = 2;
                        }

                        _foldoutStates[groupedComponent.Key].Disabled = EditorGUILayout.Foldout(
                            _foldoutStates[groupedComponent.Key].Disabled,
                            LocalizationManager.Get("editorwindow.componentmanager.disabledcomponents", disabledComponents.Count().ToString()),
                            true,
                            UnityUtils.SubTitleStyle
                        );

                        if (_foldoutStates[groupedComponent.Key].Disabled)
                        {
                            EditorGUI.indentLevel = 3;
                            foreach (var component in disabledComponents)
                            {
                                EditorGUILayout.ObjectField(component, typeof(ColorChangerForUnity), true);
                            }
                            EditorGUI.indentLevel = 2;
                        }
                    }

                    EditorGUI.indentLevel = 1;

                    EditorGUILayout.EndVertical();
                }

                var missingTextureComponents = components
                    .Where(c => c.targetTexture == null)
                    .ToList();

                if (missingTextureComponents.Count > 0)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUI.indentLevel = 1;

                    _showMissing = EditorGUILayout.Foldout(
                        _showMissing,
                        LocalizationManager.Get("editorwindow.componentmanager.texture", LocalizationManager.Get("editorwindow.componentmanager.texturemissing"), missingTextureComponents.Count.ToString()),
                        true,
                        UnityUtils.TitleStyle
                    );

                    EditorGUI.indentLevel = 2;

                    if (_showMissing)
                    {
                        EditorGUI.indentLevel = 3;
                        foreach (var component in missingTextureComponents)
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
