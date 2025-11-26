#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using net.puk06.ColorChanger.Localization;
using net.puk06.ColorChanger.Utils;
using net.puk06.TextureReplacer;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace net.puk06.ColorChanger
{
    [CustomEditor(typeof(ColorChangerForUnity))]
    [CanEditMultipleObjects]
    public class ColorChanger : Editor
    {
        #region Script Settings Serialized Property
        private SerializedProperty EnabledButtonProp => serializedObject.FindProperty("Enabled");
        private SerializedProperty PreviewEnabledButtonProp => serializedObject.FindProperty("PreviewEnabled");
        private SerializedProperty TargetTextureProp => serializedObject.FindProperty("targetTexture");
        private SerializedProperty SettingsInheritedTexturesProp => serializedObject.FindProperty("settingsInheritedTextures");
        private SerializedProperty ReplacementTextureProp => serializedObject.FindProperty("replacementTexture");
        private SerializedProperty MaskTextureProp => serializedObject.FindProperty("maskTexture");
        private SerializedProperty MaskSelectionTypeTextureProp => serializedObject.FindProperty("imageMaskSelectionType");
        #endregion

        #region Color Settings Serialized Property
        private SerializedProperty PreviousColorProp => serializedObject.FindProperty("previousColor");
        private SerializedProperty NewColorProp => serializedObject.FindProperty("newColor");
        #endregion

        #region Balance Mode Serialized Property
        private SerializedProperty BalanceModeConfigProp => serializedObject.FindProperty("balanceModeConfiguration");

        private SerializedProperty ModeVersionProp => BalanceModeConfigProp.FindPropertyRelative("ModeVersion");
        
        private SerializedProperty V1WeightProp => BalanceModeConfigProp.FindPropertyRelative("V1Weight");
        private SerializedProperty V1MinValueProp => BalanceModeConfigProp.FindPropertyRelative("V1MinimumValue");

        
        private SerializedProperty V2RadiusProp => BalanceModeConfigProp.FindPropertyRelative("V2Radius");
        private SerializedProperty V2WeightProp => BalanceModeConfigProp.FindPropertyRelative("V2Weight");
        private SerializedProperty V2MinValueProp => BalanceModeConfigProp.FindPropertyRelative("V2MinimumValue");
        private SerializedProperty V2IncludeOutsideProp => BalanceModeConfigProp.FindPropertyRelative("V2IncludeOutside");

        private SerializedProperty V3GradientProp => BalanceModeConfigProp.FindPropertyRelative("V3GradientColor");
        private SerializedProperty V3GradientPreviewResolutionProp => BalanceModeConfigProp.FindPropertyRelative("V3GradientPreviewResolution");
        private SerializedProperty V3GradientBuildResolutionProp => BalanceModeConfigProp.FindPropertyRelative("V3GradientBuildResolution");
        #endregion

        #region Advanced Color Settings Serialized Property
        private SerializedProperty AdvancedColorConfigProp => serializedObject.FindProperty("advancedColorConfiguration");
        private SerializedProperty EnabledProp => AdvancedColorConfigProp.FindPropertyRelative("Enabled");

        private SerializedProperty BrightnessProp => AdvancedColorConfigProp.FindPropertyRelative("Brightness");
        private SerializedProperty ContrastProp => AdvancedColorConfigProp.FindPropertyRelative("Contrast");
        private SerializedProperty GammaProp => AdvancedColorConfigProp.FindPropertyRelative("Gamma");
        private SerializedProperty ExposureProp => AdvancedColorConfigProp.FindPropertyRelative("Exposure");
        private SerializedProperty TransparencyProp => AdvancedColorConfigProp.FindPropertyRelative("Transparency");
        #endregion

        private bool showColorChangerSettings = false;
        private bool showTextureSettings = false;
        private bool showTargetTexturePreview = false;
        private bool showSettingsInheritedTextureSettings = false;
        private bool showMaskTextureSettings = false;
        private bool showMaskTexturePreview = false;
        private bool showTextureReplacementSettings = false;
        private bool showColorSettings = true;
        private bool showBalanceModeSettings = true;
        private bool showBalanceModeV1Settings = false;
        private bool showBalanceModeV2Settings = false;
        private bool showBalanceModeV3Settings = false;
        private bool showBalanceModeV3LUTSettings = true;
        private bool showAdvancedColorSettings = false;
        private bool showTextureOutputGui = false;
        private int selectedTextureIndex = -1;

        private enum BalanceModeSettings
        {
            None,
            V1,
            V2,
            V3
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            var logoTexture = AssetUtils.Logo;
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

            UpdateUtils.GenerateUpdateLabel();
            LocalizationUtils.GenerateLanguagePopup();

            ColorChangerForUnity? comp = target as ColorChangerForUnity;
            if (comp == null) return;

            var componentIcon = AssetUtils.Icon;
            if (componentIcon != null) EditorGUIUtility.SetIconForObject(comp, componentIcon);

            if (comp.GetComponentInParent<VRC_AvatarDescriptor>() == null)
            {
                EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.childObject.warning"), MessageType.Error);
            }
            else
            {
                UnityUtils.DrawSectionHeader(LocalizationManager.Get("editorwindow.section.scriptsettings"));

                // スクリプト設定画面
                DrawColorChangerSettingsGUI(comp);

                UnityUtils.DrawSectionHeader(LocalizationManager.Get("editorwindow.section.texturesetings"));

                // テクスチャ設定画面
                DrawTextureSettingsGUI(comp);

                // 設定を継承しているテクスチャの設定画面
                DrawSettingsInheritedTexturesSettings();

                // マスク画像設定画面
                DrawMaskTextureSettingsGUI(comp);

                // テクスチャ置き換え設定画面
                DrawTextureReplacementSettingsGUI();

                UnityUtils.DrawSectionHeader(LocalizationManager.Get("editorwindow.section.colorsetings"));

                // 色設定画面
                DrawColorSettingsGUI();

                // バランスモード画面
                DrawBalanceModeSettingsGUI();

                // 色の追加設定画面
                DrawAdvancedColorModeSettingsGUI();

                UnityUtils.DrawSectionHeader(LocalizationManager.Get("editorwindow.section.outputtexture"));

                // テクスチャ作成ボタン
                DrawTextureOutputGUI(comp);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawColorChangerSettingsGUI(ColorChangerForUnity colorChangerComponent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // インデントリセット
            EditorGUI.indentLevel = 1;

            showColorChangerSettings = EditorGUILayout.Foldout(
                showColorChangerSettings,
                LocalizationManager.Get("editorwindow.scriptsettings"),
                true,
                UnityUtils.TitleStyle
            );

            if (showColorChangerSettings)
            {
                EditorGUI.indentLevel = 2;

                EnabledButtonProp.boolValue = EditorGUILayout.Toggle(LocalizationManager.Get("editorwindow.scriptsettings.enable"), EnabledButtonProp.boolValue);
                PreviewEnabledButtonProp.boolValue = EditorGUILayout.Toggle(LocalizationManager.Get("editorwindow.scriptsettings.previewenable"), PreviewEnabledButtonProp.boolValue);

#if USE_TEXTRANSTOOL
                var mlicComponent = colorChangerComponent.GetComponentInParent<rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas>();
                var etalComponent = colorChangerComponent.GetComponent<rs64.TexTransTool.MultiLayerImage.ExternalToolAsLayer>();

                if (mlicComponent && !etalComponent)
                {
                    EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.scriptsettings.mlic.info"), MessageType.Info);
                    if (!etalComponent)
                    {
                        if (GUILayout.Button(LocalizationManager.Get("editorwindow.scriptsettings.mlic.add")))
                        {
                            Undo.AddComponent<rs64.TexTransTool.MultiLayerImage.ExternalToolAsLayer>(colorChangerComponent.gameObject);
                        }
                    }
                }

                if (!mlicComponent && etalComponent)
                {
                    EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.scriptsettings.mlic.warning"), MessageType.Warning);
                    if (GUILayout.Button(LocalizationManager.Get("editorwindow.scriptsettings.mlic.remove")))
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
                LocalizationManager.Get("editorwindow.texturesettings"),
                true,
                UnityUtils.TitleStyle
            );

            if (showTextureSettings)
            {
                TargetTextureProp.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField(LocalizationManager.Get("editorwindow.texturesettings.target"), (Texture2D)TargetTextureProp.objectReferenceValue, typeof(Texture2D), true);
                showTargetTexturePreview = EditorGUILayout.Toggle(LocalizationManager.Get("editorwindow.showpreviewtexture"), showTargetTexturePreview);

                if (showTargetTexturePreview && colorChangerComponent.targetTexture != null)
                {
                    float displayWidth = EditorGUIUtility.currentViewWidth - 40;
                    float aspect = (float)colorChangerComponent.targetTexture.height / colorChangerComponent.targetTexture.width;
                    float displayHeight = displayWidth * aspect;

                    Rect rect = GUILayoutUtility.GetRect(displayWidth, displayHeight, GUILayout.ExpandWidth(false));
                    rect.x = ((EditorGUIUtility.currentViewWidth - rect.width) / 2) + 5;

                    GUI.DrawTexture(rect, colorChangerComponent.targetTexture, ScaleMode.ScaleToFit);
                }
            }

            if (colorChangerComponent.targetTexture == null)
            {
                var gameObject = colorChangerComponent.gameObject;
                if (gameObject != null)
                {
                    var mainTexture = TextureUtils.GetMainTextureFromGameobject(gameObject);
                    if (mainTexture != null)
                    {
                        TargetTextureProp.objectReferenceValue = mainTexture;
                    }
                }
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
                LocalizationManager.Get("editorwindow.masktexturesettings"),
                true,
                UnityUtils.TitleStyle
            );

            if (showMaskTextureSettings)
            {
                EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.masktexturesettings.description"), MessageType.Info);
                MaskTextureProp.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField(LocalizationManager.Get("editorwindow.masktexturesettings.texture"), (Texture2D)MaskTextureProp.objectReferenceValue, typeof(Texture2D), true);

                string[] MaskLabels = {
                    LocalizationManager.Get("editorwindow.masktexturesettings.selectiontype.none"),
                    LocalizationManager.Get("editorwindow.masktexturesettings.selectiontype.black"),
                    LocalizationManager.Get("editorwindow.masktexturesettings.selectiontype.white"),
                    string.Format("{0} (A = 255)", LocalizationManager.Get("editorwindow.masktexturesettings.selectiontype.opaque")),
                    string.Format("{0} (A ≠ 0)", LocalizationManager.Get("editorwindow.masktexturesettings.selectiontype.opaque")),
                    string.Format("{0} (A = 0)", LocalizationManager.Get("editorwindow.masktexturesettings.selectiontype.transparent"))
                };

                MaskSelectionTypeTextureProp.enumValueIndex = EditorGUILayout.Popup(
                    new GUIContent(
                        LocalizationManager.Get("editorwindow.masktexturesettings.selectiontype"),
                        LocalizationManager.Get("editorwindow.masktexturesettings.selectiontype.description")
                    ),
                    MaskSelectionTypeTextureProp.enumValueIndex, MaskLabels
                );

                showMaskTexturePreview = EditorGUILayout.Toggle(LocalizationManager.Get("editorwindow.showpreviewtexture"), showMaskTexturePreview);

                if (showMaskTexturePreview && colorChangerComponent.maskTexture != null)
                {
                    float displayWidth = EditorGUIUtility.currentViewWidth - 40;
                    float aspect = (float)colorChangerComponent.maskTexture.height / colorChangerComponent.maskTexture.width;
                    float displayHeight = displayWidth * aspect;

                    Rect rect = GUILayoutUtility.GetRect(displayWidth, displayHeight, GUILayout.ExpandWidth(false));
                    rect.x = ((EditorGUIUtility.currentViewWidth - rect.width) / 2) + 5;

                    GUI.DrawTexture(rect, colorChangerComponent.maskTexture, ScaleMode.ScaleToFit);
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
                LocalizationManager.Get("editorwindow.settingsinheritedtexturessettings"),
                true,
                UnityUtils.TitleStyle
            );

            if (showSettingsInheritedTextureSettings)
            {
                EditorGUI.indentLevel = 2;

                EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.settingsinheritedtexturessettings.description"), MessageType.Info);
                EditorGUILayout.PropertyField(SettingsInheritedTexturesProp, true);

                EditorGUI.indentLevel = 1;
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
                LocalizationManager.Get("editorwindow.texturereplacementsettings"),
                true,
                UnityUtils.TitleStyle
            );

            if (showTextureReplacementSettings)
            {
                EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.texturereplacementsettings.description"), MessageType.Info);
                ReplacementTextureProp.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField(LocalizationManager.Get("editorwindow.texturereplacementsettings.destination"), (Texture2D)ReplacementTextureProp.objectReferenceValue, typeof(Texture2D), true);
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
                LocalizationManager.Get("editorwindow.colorsettings"),
                true,
                UnityUtils.TitleStyle
            );

            if (showColorSettings)
            {
                EditorGUI.indentLevel = 2;

                PreviousColorProp.colorValue = EditorGUILayout.ColorField(LocalizationManager.Get("editorwindow.colorsettings.previouscolor"), PreviousColorProp.colorValue);
                NewColorProp.colorValue = EditorGUILayout.ColorField(LocalizationManager.Get("editorwindow.colorsettings.newcolor"), NewColorProp.colorValue);

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
                LocalizationManager.Get("editorwindow.balancemodesettings.settings"),
                true,
                UnityUtils.TitleStyle
            );

            if (showBalanceModeSettings)
            {
                EditorGUI.indentLevel = 2;

                ModeVersionProp.intValue = (int)(BalanceModeSettings)EditorGUILayout.EnumPopup(
                    new GUIContent(
                        LocalizationManager.Get("editorwindow.balancemodesettings"),
                        LocalizationManager.Get("editorwindow.balancemodesettings.description")
                    ),
                    (BalanceModeSettings)ModeVersionProp.intValue
                );

                showBalanceModeV1Settings = EditorGUILayout.Foldout(
                    showBalanceModeV1Settings,
                    LocalizationManager.Get("editorwindow.balancemodesettings.v1"),
                    true,
                    UnityUtils.SubTitleStyle
                );

                if (showBalanceModeV1Settings)
                {
                    EditorGUI.indentLevel = 3;

                    EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.balancemodesettings.v1.description"), MessageType.Info);

                    V1WeightProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.balancemodesettings.v1.weight"), V1WeightProp.floatValue);
                    V1MinValueProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.balancemodesettings.v1.minvalue"), V1MinValueProp.floatValue);

                    EditorGUI.indentLevel = 2;
                }

                showBalanceModeV2Settings = EditorGUILayout.Foldout(
                    showBalanceModeV2Settings,
                    LocalizationManager.Get("editorwindow.balancemodesettings.v2"),
                    true,
                    UnityUtils.SubTitleStyle
                );

                if (showBalanceModeV2Settings)
                {
                    EditorGUI.indentLevel = 3;

                    EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.balancemodesettings.v2.description"), MessageType.Info);

                    V2RadiusProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.balancemodesettings.v2.radius"), V2RadiusProp.floatValue);
                    V2WeightProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.balancemodesettings.v2.weight"), V2WeightProp.floatValue);
                    V2MinValueProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.balancemodesettings.v2.minvalue"), V2MinValueProp.floatValue);
                    V2IncludeOutsideProp.boolValue = EditorGUILayout.Toggle(LocalizationManager.Get("editorwindow.balancemodesettings.v2.includeoutside"), V2IncludeOutsideProp.boolValue);

                    EditorGUI.indentLevel = 2;
                }

                showBalanceModeV3Settings = EditorGUILayout.Foldout(
                    showBalanceModeV3Settings,
                    LocalizationManager.Get("editorwindow.balancemodesettings.v3"),
                    true,
                    UnityUtils.SubTitleStyle
                );

                if (showBalanceModeV3Settings)
                {
                    EditorGUI.indentLevel = 3;

                    EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.balancemodesettings.v3.description"), MessageType.Info);

                    V3GradientProp.gradientValue = EditorGUILayout.GradientField(LocalizationManager.Get("editorwindow.balancemodesettings.v3.gradient"), V3GradientProp.gradientValue);

                    showBalanceModeV3LUTSettings = EditorGUILayout.Foldout(
                        showBalanceModeV3LUTSettings,
                        LocalizationManager.Get("editorwindow.balancemodesettings.v3.lutsetting"),
                        true,
                        UnityUtils.TitleStyle
                    );

                    if (showBalanceModeV3LUTSettings)
                    {
                        EditorGUI.indentLevel = 4;

                        EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.balancemodesettings.v3.lutdescription"), MessageType.Info);
                        
                        V3GradientPreviewResolutionProp.intValue = EditorGUILayout.IntField(LocalizationManager.Get("editorwindow.balancemodesettings.v3.previewresolution"), V3GradientPreviewResolutionProp.intValue);
                        V3GradientBuildResolutionProp.intValue = EditorGUILayout.IntField(LocalizationManager.Get("editorwindow.balancemodesettings.v3.buildresolution"), V3GradientBuildResolutionProp.intValue);

                        if (V3GradientBuildResolutionProp.intValue < V3GradientPreviewResolutionProp.intValue)
                        {
                            EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.balancemodesettings.v3.lutresolutionwarning"), MessageType.Warning);
                        }

                        EditorGUI.indentLevel = 3;
                    }

                    EditorGUI.indentLevel = 2;
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
                LocalizationManager.Get("editorwindow.advancedcolorsettings"),
                true,
                UnityUtils.TitleStyle
            );

            if (showAdvancedColorSettings)
            {
                EditorGUI.indentLevel = 2;

                EnabledProp.boolValue = EditorGUILayout.Toggle(LocalizationManager.Get("editorwindow.advancedcolorsettings.enable"), EnabledProp.boolValue);
                BrightnessProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.advancedcolorsettings.brightness"), BrightnessProp.floatValue);
                ContrastProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.advancedcolorsettings.contrast"), ContrastProp.floatValue);
                GammaProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.advancedcolorsettings.gamma"), GammaProp.floatValue);
                ExposureProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.advancedcolorsettings.exposure"), ExposureProp.floatValue);
                TransparencyProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.advancedcolorsettings.transparency"), TransparencyProp.floatValue);

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
                LocalizationManager.Get("editorwindow.textureoutputsettings"),
                true,
                UnityUtils.TitleStyle
            );

            if (showTextureOutputGui)
            {
                EditorGUI.indentLevel = 2;

                EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.textureoutputsettings.warning"), MessageType.Warning);

                var textureNames = new List<string>();
                if (colorChangerComponent.targetTexture != null) textureNames.Add($"{colorChangerComponent.targetTexture.name} - {LocalizationManager.Get("editorwindow.textureoutputsettings.texturetype.original")}");
                textureNames.AddRange(colorChangerComponent.SettingsInheritedTextures.Select(x => $"{x.name} - {LocalizationManager.Get("editorwindow.textureoutputsettings.texturetype.settingsinherited")}"));

                var textures = new List<Texture2D?>();
                if (colorChangerComponent.ComponentTexture != null) textures.Add(colorChangerComponent.ComponentTexture);
                textures.AddRange(colorChangerComponent.SettingsInheritedTextures);

                if (textures.Count == 0) return;
                if (selectedTextureIndex < 0 || selectedTextureIndex >= textures.Count) selectedTextureIndex = 0;

                // 出力するテクスチャの選択
                selectedTextureIndex = EditorGUILayout.Popup(
                    LocalizationManager.Get("editorwindow.textureoutputsettings.select"),
                    selectedTextureIndex, textureNames.ToArray()
                );

                if (GUILayout.Button(LocalizationManager.Get("editorwindow.textureoutputsettings.button"), GUILayout.ExpandWidth(true)))
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
                LogUtils.LogError(LocalizationManager.Get("editorwindow.generatetexture.missingtexture"));
                return;
            }

            try
            {
                Texture2D newTexture2D = TextureUtils.GetProcessedTexture(targetTexture, colorChangerComponent, useMask);

                string savedPath = SaveTexture(targetTexture, newTexture2D);
                DestroyImmediate(newTexture2D);

                bool confirm = EditorUtility.DisplayDialog(
                    LocalizationManager.Get("editorwindow.generatetexture.success.confirm"),
                    LocalizationManager.Get("editorwindow.generatetexture.success"),
                    LocalizationManager.Get("editorwindow.generatetexture.success.yes"),
                    LocalizationManager.Get("editorwindow.generatetexture.success.no")
                );

                if (string.IsNullOrEmpty(savedPath)) return;

                if (confirm)
                {
                    var avatarObject = colorChangerComponent.GetComponentInParent<VRC_AvatarDescriptor>().gameObject;
                    if (avatarObject != null)
                    {
                        var textureReplacerObject = new GameObject("Puko's Texture Replacer");
                        Undo.RegisterCreatedObjectUndo(textureReplacerObject, "Create Puko's Texture Replacer Object");

                        // コンポーネントの追加 + テクスチャの割り当て
                        var component = Undo.AddComponent<PukoTextureReplacer>(textureReplacerObject);
            
                        textureReplacerObject.transform.SetParent(avatarObject.transform);

                        component.sourceTexture = colorChangerComponent.targetTexture;
                        component.destinationTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(savedPath);
                    }
                    else
                    {
                        LogUtils.LogError("Couldn't find VRC Avatar Descriptor in Parent Objects.");
                    }
                }

                UnityUtils.SelectAssetAtPath(savedPath);
            }
            catch (Exception ex)
            {
                LogUtils.LogError(LocalizationManager.Get("editorwindow.generatetexture.failed", colorChangerComponent.name, ex.ToString()));
            }
        }

        private string SaveTexture(Texture2D originalTexture, Texture2D newTexture)
        {
            string originalPath = AssetDatabase.GetAssetPath(originalTexture);
            if (string.IsNullOrEmpty(originalPath))
            {
                LogUtils.LogError(LocalizationManager.Get("editorwindow.generatetexture.save.missingpath"));
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
                LogUtils.LogError(LocalizationManager.Get("editorwindow.generatetexture.save.encodefailed"));
                return string.Empty;
            }

            File.WriteAllBytes(savePath, pngData);
            LogUtils.Log(LocalizationManager.Get("editorwindow.generatetexture.save.success", savePath));

            AssetDatabase.ImportAsset(savePath);
            AssetDatabase.Refresh();

            return savePath;
        }
    }
}
