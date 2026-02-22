#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using net.puk06.ColorChanger.Editor.Models;
using net.puk06.ColorChanger.Editor.Services;
using net.puk06.ColorChanger.Editor.Utils;
using net.puk06.TextureReplacer;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace net.puk06.ColorChanger.Editor
{
    [CustomEditor(typeof(ColorChangerForUnity))]
    [CanEditMultipleObjects]
    internal class ColorChangerInspector : UnityEditor.Editor
    {
        private ExtendedRenderTexture? _previewTexture;
        private void OnEnable() => UpdatePreview();
        private void OnDisable() => DisposeTexture();
        private void OnValidate() => ReloadPreviewInstances();

        public override bool HasPreviewGUI() => true;
        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            if (_previewTexture != null)
            {
                GUI.DrawTexture(rect, _previewTexture, ScaleMode.ScaleToFit);
            }
        }

        public ExtendedRenderTexture? GeneratePreview()
        {
            ColorChangerForUnity? component = target as ColorChangerForUnity;
            if (component == null || component.ComponentTexture == null) return null;

            return CCTextureBuilder.Build(component.ComponentTexture, component, true);
        }

        public void DisposeTexture()
        {
            if (_previewTexture != null) _previewTexture.Dispose();
        }

        public void UpdatePreview()
        {
            DisposeTexture();
            _previewTexture = GeneratePreview();
        }

        private bool showColorChangerSettings = false;
        private bool showTextureSettings = false;
        private bool showTargetTexturePreview = false;
        private bool showSettingsInheritedTextureSettings = false;
        private bool showMaskTextureSettings = false;
        private bool showMaskTexturePreview = false;
        private bool showTextureReplacementSettings = false;
        private bool showColorSettings = true;
        private bool showBalanceModeSettings = true;
        private bool showAdvancedColorSettings = false;
        private bool showTextureOutputGui = false;
        private int selectedTextureIndex = -1;

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            var logoTexture = ComponentAssetsLoader.Logo;
            if (logoTexture != null)
            {
                float imgWidth = logoTexture.width;
                float imgHeight = logoTexture.height;

                float viewWidth = EditorGUIUtility.currentViewWidth;

                float aspectRatio = imgHeight / imgWidth;
                float displayWidth = viewWidth;
                float displayHeight = displayWidth * aspectRatio;

                Rect rect = GUILayoutUtility.GetRect(displayWidth, displayHeight, GUILayout.ExpandWidth(true));

                GUI.DrawTexture(rect, logoTexture, ScaleMode.ScaleToFit);
            }

            UpdateUtils.GenerateVersionLabel();
            LocalizationUtils.DrawLanguageSelectionPopup();

            ColorChangerForUnity? comp = target as ColorChangerForUnity;
            if (comp == null) return;

            var componentIcon = ComponentAssetsLoader.Icon;
            if (componentIcon != null) EditorGUIUtility.SetIconForObject(comp, componentIcon);

            if (comp.GetComponentInParent<VRC_AvatarDescriptor>() == null)
            {
                EditorGUILayout.HelpBox(LocalizationUtils.Localize("editorwindow.childObject.warning"), MessageType.Error);
            }
            else
            {
                UnityService.DrawSectionHeader(LocalizationUtils.Localize("Inspector.Section.Script"));
                DrawColorChangerSettingsGUI(comp);

                UnityService.DrawSectionHeader(LocalizationUtils.Localize("Inspector.Section.Texture"));

                DrawTextureSettingsGUI(comp);
                DrawSettingsInheritedTexturesSettings();
                DrawMaskTextureSettingsGUI(comp);
                DrawTextureReplacementSettingsGUI();

                UnityService.DrawSectionHeader(LocalizationUtils.Localize("Inspector.Section.Color"));

                // 色設定画面
                DrawColorSettingsGUI();

                // バランスモード画面
                DrawBalanceModeSettingsGUI();

                // 色の追加設定画面
                DrawAdvancedColorModeSettingsGUI();

                UnityService.DrawSectionHeader(LocalizationUtils.Localize("Inspector.Section.TextureOutput"));

                // テクスチャ作成ボタン
                DrawTextureOutputGUI(comp);
            }

            bool changed = serializedObject.ApplyModifiedProperties();
            if (changed) UpdatePreview();
        }

        private void DrawColorChangerSettingsGUI(ColorChangerForUnity colorChangerComponent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel = 1;

            showColorChangerSettings = EditorGUILayout.Foldout(
                showColorChangerSettings,
                LocalizationUtils.Localize("Inspector.Script.Section.ScriptConfiguration"),
                true,
                UnityService.TitleStyle
            );

            if (showColorChangerSettings)
            {
                EditorGUI.indentLevel = 2;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Enabled"), new GUIContent(LocalizationUtils.Localize("Inspector.Script.ScriptConfiguration.IsEnabled")));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewEnabled"), new GUIContent(LocalizationUtils.Localize("Inspector.Script.ScriptConfiguration.IsPreviewEnabled")));

#if USE_TEXTRANSTOOL
                var mlicComponent = colorChangerComponent.GetComponentInParent<rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas>();
                var etalComponent = colorChangerComponent.GetComponent<rs64.TexTransTool.MultiLayerImage.ExternalToolAsLayer>();

                if (mlicComponent && !etalComponent)
                {
                    EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.Script.ScriptConfiguration.TexTransTool.MLIC.Info"), MessageType.Info);
                    if (!etalComponent)
                    {
                        if (GUILayout.Button(LocalizationUtils.Localize("Inspector.Script.ScriptConfiguration.TexTransTool.MLIC.AddComponent")))
                        {
                            Undo.AddComponent<rs64.TexTransTool.MultiLayerImage.ExternalToolAsLayer>(colorChangerComponent.gameObject);
                        }
                    }
                }

                if (!mlicComponent && etalComponent)
                {
                    EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.Script.ScriptConfiguration.TexTransTool.MLIC.Warning"), MessageType.Warning);
                    if (GUILayout.Button(LocalizationUtils.Localize("Inspector.Script.ScriptConfiguration.TexTransTool.MLIC.RemoveComponent")))
                    {
                        Undo.DestroyObjectImmediate(etalComponent);
                    }
                }
#endif

                EditorGUI.indentLevel = 1;
            }

            EditorGUI.indentLevel = 0;

            EditorGUILayout.EndVertical();
        }

        private void DrawTextureSettingsGUI(ColorChangerForUnity colorChangerComponent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel = 1;

            showTextureSettings = EditorGUILayout.Foldout(
                showTextureSettings,
                LocalizationUtils.Localize("Inspector.Texture.Section.TextureConfiguration"),
                true,
                UnityService.TitleStyle
            );

            SerializedProperty TargetTextureProp = serializedObject.FindProperty("TargetTexture");

            if (showTextureSettings)
            {
                TargetTextureProp.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField(LocalizationUtils.Localize("Inspector.Texture.TextureConfiguration.TargetTexture"), (Texture2D)TargetTextureProp.objectReferenceValue, typeof(Texture2D), true);
                showTargetTexturePreview = EditorGUILayout.Toggle(LocalizationUtils.Localize("Inspector.Texture.TextureConfiguration.ShowPreview"), showTargetTexturePreview);

                if (showTargetTexturePreview && colorChangerComponent.TargetTexture != null)
                {
                    float displayWidth = EditorGUIUtility.currentViewWidth - 40;
                    float aspect = (float)colorChangerComponent.TargetTexture.height / colorChangerComponent.TargetTexture.width;
                    float displayHeight = displayWidth * aspect;

                    Rect rect = GUILayoutUtility.GetRect(displayWidth, displayHeight, GUILayout.ExpandWidth(false));
                    rect.x = ((EditorGUIUtility.currentViewWidth - rect.width) / 2) + 5;

                    GUI.DrawTexture(rect, colorChangerComponent.TargetTexture, ScaleMode.ScaleToFit);
                }
            }

            EditorGUI.indentLevel = 0;

            EditorGUILayout.EndVertical();
        }

        private void DrawSettingsInheritedTexturesSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel = 1;

            showSettingsInheritedTextureSettings = EditorGUILayout.Foldout(
                showSettingsInheritedTextureSettings,
                LocalizationUtils.Localize("Inspector.Texture.Section.SettingsInheritedTextures"),
                true,
                UnityService.TitleStyle
            );

            if (showSettingsInheritedTextureSettings)
            {
                EditorGUI.indentLevel = 2;

                EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.Texture.SettingsInheritedTextures.Description"), MessageType.Info);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SettingsInheritedTextures"), true);

                EditorGUI.indentLevel = 1;
            }

            EditorGUI.indentLevel = 0;

            EditorGUILayout.EndVertical();
        }

        private void DrawMaskTextureSettingsGUI(ColorChangerForUnity colorChangerComponent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel = 1;

            showMaskTextureSettings = EditorGUILayout.Foldout(
                showMaskTextureSettings,
                LocalizationUtils.Localize("Inspector.Texture.Section.MaskTexture"),
                true,
                UnityService.TitleStyle
            );

            SerializedProperty MaskTextureProp = serializedObject.FindProperty("MaskTexture");

            if (showMaskTextureSettings)
            {
                EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.Texture.MaskTexture.Description"), MessageType.Info);
                MaskTextureProp.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField(LocalizationUtils.Localize("Inspector.Texture.MaskTexture.Texture"), (Texture2D)MaskTextureProp.objectReferenceValue, typeof(Texture2D), true);

                string[] MaskLabels = {
                    LocalizationUtils.Localize("Inspector.Texture.MaskTexture.ImageMaskSelectionType.Options.None"),
                    LocalizationUtils.Localize("Inspector.Texture.MaskTexture.ImageMaskSelectionType.Options.Black"),
                    LocalizationUtils.Localize("Inspector.Texture.MaskTexture.ImageMaskSelectionType.Options.White"),
                    string.Format("{0} (A = 255)", LocalizationUtils.Localize("Inspector.Texture.MaskTexture.ImageMaskSelectionType.Options.Opaque")),
                    string.Format("{0} (A ≠ 0)", LocalizationUtils.Localize("Inspector.Texture.MaskTexture.ImageMaskSelectionType.Options.Opaque")),
                    string.Format("{0} (A = 0)", LocalizationUtils.Localize("Inspector.Texture.MaskTexture.ImageMaskSelectionType.Options.Transparent"))
                };

                SerializedProperty MaskSelectionTypeTextureProp = serializedObject.FindProperty("ImageMaskSelectionType");

                MaskSelectionTypeTextureProp.enumValueIndex = EditorGUILayout.Popup(
                    new GUIContent(
                        LocalizationUtils.Localize("Inspector.Texture.MaskTexture.ImageMaskSelectionType"),
                        LocalizationUtils.Localize("Inspector.Texture.MaskTexture.ImageMaskSelectionType.Description")
                    ),
                    MaskSelectionTypeTextureProp.enumValueIndex, MaskLabels
                );

                showMaskTexturePreview = EditorGUILayout.Toggle(LocalizationUtils.Localize("Inspector.Texture.MaskTexture.ShowPreview"), showMaskTexturePreview);

                if (showMaskTexturePreview && colorChangerComponent.MaskTexture != null)
                {
                    float displayWidth = EditorGUIUtility.currentViewWidth - 40;
                    float aspect = (float)colorChangerComponent.MaskTexture.height / colorChangerComponent.MaskTexture.width;
                    float displayHeight = displayWidth * aspect;

                    Rect rect = GUILayoutUtility.GetRect(displayWidth, displayHeight, GUILayout.ExpandWidth(false));
                    rect.x = ((EditorGUIUtility.currentViewWidth - rect.width) / 2) + 5;

                    GUI.DrawTexture(rect, colorChangerComponent.MaskTexture, ScaleMode.ScaleToFit);
                }
            }

            EditorGUI.indentLevel = 0;

            EditorGUILayout.EndVertical();
        }

        private void DrawTextureReplacementSettingsGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel = 1;

            showTextureReplacementSettings = EditorGUILayout.Foldout(
                showTextureReplacementSettings,
                LocalizationUtils.Localize("Inspector.Texture.Section.TexureReplacement"),
                true,
                UnityService.TitleStyle
            );

            if (showTextureReplacementSettings)
            {
                SerializedProperty ReplacementTextureProp = serializedObject.FindProperty("ReplacementTexture");
                EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.Texture.TexureReplacement.Description"), MessageType.Info);
                ReplacementTextureProp.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField(LocalizationUtils.Localize("Inspector.Texture.TexureReplacement.TargetTexture"), (Texture2D)ReplacementTextureProp.objectReferenceValue, typeof(Texture2D), true);
            }

            EditorGUI.indentLevel = 0;

            EditorGUILayout.EndVertical();
        }

        private void DrawColorSettingsGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel = 1;

            showColorSettings = EditorGUILayout.Foldout(
                showColorSettings,
                LocalizationUtils.Localize("Inspector.Color.Section.ColorConfiguration"),
                true,
                UnityService.TitleStyle
            );

            if (showColorSettings)
            {
                EditorGUI.indentLevel = 2;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("SourceColor"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.ColorConfiguration.SourceColor")));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("TargetColor"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.ColorConfiguration.TargetColor")));

                EditorGUI.indentLevel = 1;
            }

            EditorGUI.indentLevel = 0;

            EditorGUILayout.EndVertical();
        }

        private void DrawBalanceModeSettingsGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel = 1;

            showBalanceModeSettings = EditorGUILayout.Foldout(
                showBalanceModeSettings,
                LocalizationUtils.Localize("Inspector.Color.Section.BalanceModeConfiguration"),
                true,
                UnityService.TitleStyle
            );

            if (showBalanceModeSettings)
            {
                EditorGUI.indentLevel = 2;

                EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.Description"), MessageType.Info);

                SerializedProperty BalanceModeConfigProp = serializedObject.FindProperty("BalanceModeConfiguration");

                string[] balanceModeLabels =
                {
                    LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.ModeVersion.Options.None"),
                    LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.ModeVersion.Options.V1"),
                    LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.ModeVersion.Options.V2"),
                    LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.ModeVersion.Options.V3"),
                };

                SerializedProperty modeProperty = BalanceModeConfigProp.FindPropertyRelative("ModeVersion");
                modeProperty.intValue = EditorGUILayout.Popup(LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.ModeVersion"), modeProperty.intValue, balanceModeLabels);

                switch (modeProperty.intValue)
                {
                    case 1:
                        {
                            EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.V1.Description"), MessageType.Info);
                            EditorGUILayout.PropertyField(BalanceModeConfigProp.FindPropertyRelative("V1Weight"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.V1.Weight")));
                            EditorGUILayout.PropertyField(BalanceModeConfigProp.FindPropertyRelative("V1MinimumValue"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.V1.Minimum")));
                            break;
                        }

                    case 2:
                        {
                            EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.V2.Description"), MessageType.Info);
                            EditorGUILayout.PropertyField(BalanceModeConfigProp.FindPropertyRelative("V2Radius"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.V2.Radius")));
                            EditorGUILayout.PropertyField(BalanceModeConfigProp.FindPropertyRelative("V2Weight"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.V2.Weight")));
                            EditorGUILayout.PropertyField(BalanceModeConfigProp.FindPropertyRelative("V2MinimumValue"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.V2.Minimum")));
                            EditorGUILayout.PropertyField(BalanceModeConfigProp.FindPropertyRelative("V2IncludeOutside"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.V2.IncludeOutside")));
                            break;
                        }

                    case 3:
                        {
                            EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.V3.Description"), MessageType.Info);
                            EditorGUILayout.PropertyField(BalanceModeConfigProp.FindPropertyRelative("V3Gradient"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.V3.Gradient")));
                            EditorGUILayout.PropertyField(BalanceModeConfigProp.FindPropertyRelative("V3GradientResolution"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.BalanceModeConfiguration.V3.LUTConfiguration.Resolution")));
                            break;
                        }

                }
            }

            EditorGUI.indentLevel = 1;

            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedColorModeSettingsGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // インデントリセット
            EditorGUI.indentLevel = 1;

            showAdvancedColorSettings = EditorGUILayout.Foldout(
                showAdvancedColorSettings,
                LocalizationUtils.Localize("Inspector.Color.Section.AdvancedColorConfiguration"),
                true,
                UnityService.TitleStyle
            );

            if (showAdvancedColorSettings)
            {
                EditorGUI.indentLevel = 2;

                SerializedProperty AdvancedColorConfigProp = serializedObject.FindProperty("AdvancedColorConfiguration");

                EditorGUILayout.PropertyField(AdvancedColorConfigProp.FindPropertyRelative("IsEnabled"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.AdvancedColorConfiguration.IsEnabled")));
                EditorGUILayout.PropertyField(AdvancedColorConfigProp.FindPropertyRelative("Hue"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.AdvancedColorConfiguration.Hue")));
                EditorGUILayout.PropertyField(AdvancedColorConfigProp.FindPropertyRelative("Saturation"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.AdvancedColorConfiguration.Saturation")));
                EditorGUILayout.PropertyField(AdvancedColorConfigProp.FindPropertyRelative("Value"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.AdvancedColorConfiguration.Value")));
                EditorGUILayout.PropertyField(AdvancedColorConfigProp.FindPropertyRelative("Brightness"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.AdvancedColorConfiguration.Brightness")));
                EditorGUILayout.PropertyField(AdvancedColorConfigProp.FindPropertyRelative("Contrast"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.AdvancedColorConfiguration.Contrast")));
                EditorGUILayout.PropertyField(AdvancedColorConfigProp.FindPropertyRelative("Gamma"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.AdvancedColorConfiguration.Gamma")));
                EditorGUILayout.PropertyField(AdvancedColorConfigProp.FindPropertyRelative("Exposure"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.AdvancedColorConfiguration.Exposure")));
                EditorGUILayout.PropertyField(AdvancedColorConfigProp.FindPropertyRelative("Transparency"), new GUIContent(LocalizationUtils.Localize("Inspector.Color.AdvancedColorConfiguration.Transparency")));

                EditorGUI.indentLevel = 1;
            }

            EditorGUI.indentLevel = 0;
            EditorGUILayout.EndVertical();
        }

        private void DrawTextureOutputGUI(ColorChangerForUnity colorChangerComponent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel = 1;

            showTextureOutputGui = EditorGUILayout.Foldout(
                showTextureOutputGui,
                LocalizationUtils.Localize("Inspector.TextureOutput.Section.TextureOutputConfigurtion"),
                true,
                UnityService.TitleStyle
            );

            if (showTextureOutputGui)
            {
                EditorGUI.indentLevel = 2;

                EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.TextureOutput.TextureOutputConfigurtion.Warning"), MessageType.Warning);

                List<string> textureNames = new();
                if (colorChangerComponent.TargetTexture != null) textureNames.Add($"{colorChangerComponent.TargetTexture.name} - {LocalizationUtils.Localize("Inspector.TextureOutput.TextureOutputConfigurtion.TextureType.OriginalTexture")}");
                textureNames.AddRange(colorChangerComponent.SettingsInheritedTextures.Where(t => t != null).Select(x => $"{x!.name} - {LocalizationUtils.Localize("Inspector.TextureOutput.TextureOutputConfigurtion.TextureType.SettingsInheritedTexture")}"));

                List<Texture2D?> textures = new();
                if (colorChangerComponent.ComponentTexture != null) textures.Add(colorChangerComponent.ComponentTexture);
                textures.AddRange(colorChangerComponent.SettingsInheritedTextures.Where(t => t != null));

                if (textures.Count == 0) return;
                if (selectedTextureIndex < 0 || selectedTextureIndex >= textures.Count) selectedTextureIndex = 0;

                // 出力するテクスチャの選択
                selectedTextureIndex = EditorGUILayout.Popup(
                    LocalizationUtils.Localize("Inspector.TextureOutput.TextureOutputConfigurtion.SelectTexure"),
                    selectedTextureIndex, textureNames.ToArray()
                );

                if (GUILayout.Button(LocalizationUtils.Localize("Inspector.TextureOutput.TextureOutputConfigurtion.Output"), GUILayout.ExpandWidth(true)))
                {
                    GenerateTexture(colorChangerComponent, textures[selectedTextureIndex], selectedTextureIndex == 0);
                }

                EditorGUI.indentLevel = 1;
            }

            EditorGUI.indentLevel = 0;

            EditorGUILayout.EndVertical();
        }

        private void GenerateTexture(ColorChangerForUnity colorChangerComponent, Texture2D? targetTexture, bool useMask)
        {
            if (targetTexture == null)
            {
                LogUtils.LogError(LocalizationUtils.Localize("Inspector.TextureOutput.TextureOutputConfigurtion.Error.MissingTexture"));
                return;
            }

            try
            {
                ExtendedRenderTexture? processedRenderTexture = CCTextureBuilder.Build(targetTexture, colorChangerComponent, useMask);
                if (processedRenderTexture == null)
                {
                    LogUtils.LogError(string.Format(LocalizationUtils.Localize("Inspector.TextureOutput.TextureOutputConfigurtion.Failed"), colorChangerComponent.name));
                    return;
                }

                Texture2D? processedTexture = processedRenderTexture.ToTexture2D();
                processedRenderTexture.Dispose();

                string savedPath = SaveTexture(targetTexture, processedTexture);
                DestroyImmediate(processedRenderTexture);

                bool confirm = EditorUtility.DisplayDialog(
                    LocalizationUtils.Localize("Inspector.TextureOutput.TextureOutputConfigurtion.Success.Replace.Confirm"),
                    LocalizationUtils.Localize("Inspector.TextureOutput.TextureOutputConfigurtion.Success.Replace"),
                    LocalizationUtils.Localize("Inspector.TextureOutput.TextureOutputConfigurtion.Success.Replace.Yes"),
                    LocalizationUtils.Localize("Inspector.TextureOutput.TextureOutputConfigurtion.Success.Replace.No")
                );

                if (string.IsNullOrEmpty(savedPath)) return;

                if (confirm)
                {
                    var avatarObject = colorChangerComponent.GetComponentInParent<VRC_AvatarDescriptor>().gameObject;
                    if (avatarObject != null)
                    {
                        GameObject textureReplacerObject = new("Puko's Texture Replacer");
                        Undo.RegisterCreatedObjectUndo(textureReplacerObject, "Create Puko's Texture Replacer Object");

                        // コンポーネントの追加 + テクスチャの割り当て
                        PukoTextureReplacer component = Undo.AddComponent<PukoTextureReplacer>(textureReplacerObject);

                        textureReplacerObject.transform.SetParent(avatarObject.transform);

                        component.sourceTexture = colorChangerComponent.TargetTexture;
                        component.destinationTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(savedPath);
                    }
                    else
                    {
                        LogUtils.LogError("Couldn't find VRC Avatar Descriptor in Parent Objects.");
                    }
                }

                UnityService.SelectAssetAtPath(savedPath);
            }
            catch (Exception ex)
            {
                LogUtils.LogError(string.Format(LocalizationUtils.Localize("editorwindow.generatetexture.failed"), colorChangerComponent.name, ex.ToString()));
            }
        }

        private string SaveTexture(Texture2D originalTexture, Texture2D newTexture)
        {
            string originalPath = AssetDatabase.GetAssetPath(originalTexture);
            if (string.IsNullOrEmpty(originalPath))
            {
                LogUtils.LogError(LocalizationUtils.Localize("Inspector.TextureOutput.TextureOutputConfigurtion.Error.MissingTexturePath"));
                return string.Empty;
            }

            string directory = Path.GetDirectoryName(originalPath);
            string originalFileName = Path.GetFileNameWithoutExtension(originalPath);
            string extension = ".png";

            int index = 1;
            string savePath;
            do
            {
                string fileName = $"{originalFileName} {index}{extension}";
                savePath = Path.Combine(directory, fileName);
                index++;
            } while (File.Exists(savePath));

            byte[] pngData = newTexture.EncodeToPNG();
            if (pngData == null)
            {
                LogUtils.LogError(LocalizationUtils.Localize("Inspector.TextureOutput.TextureOutputConfigurtion.Error.EncodeFailed"));
                return string.Empty;
            }

            File.WriteAllBytes(savePath, pngData);
            LogUtils.Log(string.Format(LocalizationUtils.Localize("Inspector.TextureOutput.TextureOutputConfigurtion.Success.Save"), savePath));

            AssetDatabase.ImportAsset(savePath);
            AssetDatabase.Refresh();

            return savePath;
        }
    }
}
