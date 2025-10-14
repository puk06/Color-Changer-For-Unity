#nullable enable
using System;
using System.IO;
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
        private SerializedProperty targetTextureProp = null!;
        private SerializedProperty previousColorProp = null!;
        private SerializedProperty newColorProp = null!;

        private SerializedProperty balanceModeConfigProp = null!;
        private SerializedProperty advancedColorConfigProp = null!;

        private bool showColorChangerSettings = true;
        private bool showTextureSettings = false;
        private bool showColorSettings = true;
        private bool showBalanceModeSettings = true;
        private bool showBalanceModeV1Settings = false;
        private bool showBalanceModeV2Settings = false;
        private bool showBalanceModeV3Settings = false;

        private bool showAdvancedColorSettings = false;

        private enum BalanceModeSettings
        {
            None,
            V1,
            V2,
            V3
        }

        void OnEnable()
        {
            targetTextureProp = serializedObject.FindProperty("targetTexture");
            previousColorProp = serializedObject.FindProperty("previousColor");
            newColorProp = serializedObject.FindProperty("newColor");

            balanceModeConfigProp = serializedObject.FindProperty("balanceModeConfiguration");
            advancedColorConfigProp = serializedObject.FindProperty("advancedColorConfiguration");
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
                // スクリプト設定画面
                DrawColorChangerSettingsGUI(comp);

                // テクスチャ設定画面
                DrawTextureSettingsGUI(comp);

                // 色設定画面
                DrawColorSettingsGUI();

                // バランスモード画面
                DrawBalanceModeSettingsGUI();

                // 色の追加設定画面
                DrawAdvancedColorModeSettingsGUI();

                EditorGUILayout.Space(10);

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
                LocalizationManager.Get("editorwindow.scriptsetting"),
                true,
                UnityUtils.TitleStyle
            );

            if (showColorChangerSettings)
            {
                EditorGUI.indentLevel = 2;

                SerializedProperty enabledButtonProp = serializedObject.FindProperty("Enabled");
                SerializedProperty previewEnabledButtonProp = serializedObject.FindProperty("PreviewEnabled");
                SerializedProperty previewOnCPUButtonProp = serializedObject.FindProperty("PreviewOnCPU");

                enabledButtonProp.boolValue = EditorGUILayout.Toggle(LocalizationManager.Get("editorwindow.scriptsetting.enable"), enabledButtonProp.boolValue);
                previewEnabledButtonProp.boolValue = EditorGUILayout.Toggle(LocalizationManager.Get("editorwindow.scriptsetting.previewenable"), previewEnabledButtonProp.boolValue);

                EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.scriptsetting.cpurendering.warning"), MessageType.Warning);
                previewOnCPUButtonProp.boolValue = EditorGUILayout.Toggle(LocalizationManager.Get("editorwindow.scriptsetting.cpurendering.enable"), previewOnCPUButtonProp.boolValue);

#if USE_TEXTRANSTOOL
                var mlicComponent = colorChangerComponent.GetComponentInParent<rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas>();
                var etalComponent = colorChangerComponent.GetComponent<rs64.TexTransTool.MultiLayerImage.ExternalToolAsLayer>();

                if (mlicComponent && !etalComponent)
                {
                    EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.scriptsetting.mlic.info"), MessageType.Info);
                    if (!etalComponent)
                    {
                        if (GUILayout.Button(LocalizationManager.Get("editorwindow.scriptsetting.mlic.add")))
                        {
                            Undo.AddComponent<rs64.TexTransTool.MultiLayerImage.ExternalToolAsLayer>(colorChangerComponent.gameObject);
                        }
                    }
                }

                if (!mlicComponent && etalComponent)
                {
                    EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.scriptsetting.mlic.warning"), MessageType.Warning);
                    if (GUILayout.Button(LocalizationManager.Get("editorwindow.scriptsetting.mlic.remove")))
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
                LocalizationManager.Get("editorwindow.texturesetting"),
                true,
                UnityUtils.TitleStyle
            );

            if (showTextureSettings)
            {
                targetTextureProp.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField(LocalizationManager.Get("editorwindow.texturesetting.target"), (Texture2D)targetTextureProp.objectReferenceValue, typeof(Texture2D), true);

                if (colorChangerComponent.targetTexture != null)
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
                        targetTextureProp.objectReferenceValue = mainTexture;
                    }
                }
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
                LocalizationManager.Get("editorwindow.colorsetting"),
                true,
                UnityUtils.TitleStyle
            );

            if (showColorSettings)
            {
                EditorGUI.indentLevel = 2;

                previousColorProp.colorValue = EditorGUILayout.ColorField(LocalizationManager.Get("editorwindow.colorsetting.previouscolor"), previousColorProp.colorValue);
                newColorProp.colorValue = EditorGUILayout.ColorField(LocalizationManager.Get("editorwindow.colorsetting.newcolor"), newColorProp.colorValue);

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
                LocalizationManager.Get("editorwindow.balancemode.setting"),
                true,
                UnityUtils.TitleStyle
            );

            if (showBalanceModeSettings)
            {
                EditorGUI.indentLevel = 2;

                SerializedProperty modeVersionProp = balanceModeConfigProp.FindPropertyRelative("ModeVersion");
                modeVersionProp.intValue = (int)(BalanceModeSettings)EditorGUILayout.EnumPopup(
                    new GUIContent(
                        LocalizationManager.Get("editorwindow.balancemode"),
                        LocalizationManager.Get("editorwindow.balancemode.description")
                    ),
                    (BalanceModeSettings)modeVersionProp.intValue
                );

                showBalanceModeV1Settings = EditorGUILayout.Foldout(
                    showBalanceModeV1Settings,
                    LocalizationManager.Get("editorwindow.balancemode.v1"),
                    true,
                    UnityUtils.SubTitleStyle
                );

                if (showBalanceModeV1Settings)
                {
                    EditorGUI.indentLevel = 3;

                    EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.balancemode.v1.description"), MessageType.Info);

                    SerializedProperty v1WeightProp = balanceModeConfigProp.FindPropertyRelative("V1Weight");
                    SerializedProperty v1MinValueProp = balanceModeConfigProp.FindPropertyRelative("V1MinimumValue");

                    v1WeightProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.balancemode.v1.weight"), v1WeightProp.floatValue);
                    v1MinValueProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.balancemode.v1.minvalue"), v1MinValueProp.floatValue);

                    EditorGUI.indentLevel = 2;
                }

                showBalanceModeV2Settings = EditorGUILayout.Foldout(
                    showBalanceModeV2Settings,
                    LocalizationManager.Get("editorwindow.balancemode.v2"),
                    true,
                    UnityUtils.SubTitleStyle
                );

                if (showBalanceModeV2Settings)
                {
                    EditorGUI.indentLevel = 3;

                    EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.balancemode.v2.description"), MessageType.Info);

                    SerializedProperty v2RadiusProp = balanceModeConfigProp.FindPropertyRelative("V2Radius");
                    SerializedProperty v2WeightProp = balanceModeConfigProp.FindPropertyRelative("V2Weight");
                    SerializedProperty v2MinValueProp = balanceModeConfigProp.FindPropertyRelative("V2MinimumValue");
                    SerializedProperty v2IncludeOutsideProp = balanceModeConfigProp.FindPropertyRelative("V2IncludeOutside");

                    v2RadiusProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.balancemode.v2.radius"), v2RadiusProp.floatValue);
                    v2WeightProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.balancemode.v2.weight"), v2WeightProp.floatValue);
                    v2MinValueProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.balancemode.v2.minvalue"), v2MinValueProp.floatValue);
                    v2IncludeOutsideProp.boolValue = EditorGUILayout.Toggle(LocalizationManager.Get("editorwindow.balancemode.v2.includeoutside"), v2IncludeOutsideProp.boolValue);

                    EditorGUI.indentLevel = 2;
                }

                showBalanceModeV3Settings = EditorGUILayout.Foldout(
                    showBalanceModeV3Settings,
                    LocalizationManager.Get("editorwindow.balancemode.v3"),
                    true,
                    UnityUtils.SubTitleStyle
                );

                if (showBalanceModeV3Settings)
                {
                    EditorGUI.indentLevel = 3;

                    EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.balancemode.v3.description"), MessageType.Info);

                    SerializedProperty v3GradientProp = balanceModeConfigProp.FindPropertyRelative("V3GradientColor");
                    SerializedProperty v3GradientResolutionProp = balanceModeConfigProp.FindPropertyRelative("V3GradientPreviewResolution");

                    v3GradientProp.gradientValue = EditorGUILayout.GradientField(LocalizationManager.Get("editorwindow.balancemode.v3.gradient"), v3GradientProp.gradientValue);
                    v3GradientResolutionProp.intValue = EditorGUILayout.IntField(LocalizationManager.Get("editorwindow.balancemode.v3.previewresolution"), v3GradientResolutionProp.intValue);

                    EditorGUI.indentLevel = 2;
                }
            }

            EditorGUI.indentLevel = 1;

            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedColorModeSettingsGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.foldout);
            titleStyle.fontSize = 13;
            titleStyle.fontStyle = FontStyle.Bold;

            // インデントリセット
            EditorGUI.indentLevel = 1;

            showAdvancedColorSettings = EditorGUILayout.Foldout(showAdvancedColorSettings, LocalizationManager.Get("editorwindow.advancedsettings"), true, titleStyle);
            if (showAdvancedColorSettings)
            {
                EditorGUI.indentLevel = 2;

                SerializedProperty enabledProp = advancedColorConfigProp.FindPropertyRelative("Enabled");
                SerializedProperty brightnessProp = advancedColorConfigProp.FindPropertyRelative("Brightness");
                SerializedProperty contrastProp = advancedColorConfigProp.FindPropertyRelative("Contrast");
                SerializedProperty gammaProp = advancedColorConfigProp.FindPropertyRelative("Gamma");
                SerializedProperty exposureProp = advancedColorConfigProp.FindPropertyRelative("Exposure");
                SerializedProperty transparencyProp = advancedColorConfigProp.FindPropertyRelative("Transparency");

                enabledProp.boolValue = EditorGUILayout.Toggle(LocalizationManager.Get("editorwindow.advancedsettings.enable"), enabledProp.boolValue);
                brightnessProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.advancedsettings.brightness"), brightnessProp.floatValue);
                contrastProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.advancedsettings.contrast"), contrastProp.floatValue);
                gammaProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.advancedsettings.gamma"), gammaProp.floatValue);
                exposureProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.advancedsettings.exposure"), exposureProp.floatValue);
                transparencyProp.floatValue = EditorGUILayout.FloatField(LocalizationManager.Get("editorwindow.advancedsettings.transparency"), transparencyProp.floatValue);

                EditorGUI.indentLevel = 1;
            }

            EditorGUI.indentLevel = 0;
            EditorGUILayout.EndVertical();
        }

        private void DrawTextureOutputGUI(ColorChangerForUnity colorChangerComponent)
        {
            EditorGUI.indentLevel = 0;
            EditorGUILayout.HelpBox(LocalizationManager.Get("editorwindow.textureoutput.warning"), MessageType.Warning);
            if (GUILayout.Button(LocalizationManager.Get("editorwindow.textureoutput.button"), GUILayout.ExpandWidth(true)))
            {
                GenerateTexture(colorChangerComponent);
            }
        }

        private void GenerateTexture(ColorChangerForUnity colorChangerComponent)
        {
            if (colorChangerComponent.targetTexture == null)
            {
                LogUtils.LogError(LocalizationManager.Get("editorwindow.generatetexture.missingtexture"));
                return;
            }

            Texture2D? originalTexture = null;
            Texture2D? newTexture = null;

            try
            {
                originalTexture = TextureUtils.GetRawTexture(colorChangerComponent.targetTexture);
                newTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false, false);

                TextureUtils.ProcessTexture(originalTexture, newTexture, colorChangerComponent);

                string savedPath = SaveTexture(colorChangerComponent.targetTexture, newTexture);

                bool confirm = EditorUtility.DisplayDialog(
                    LocalizationManager.Get("editorwindow.generatetexture.success.confirm"),
                    LocalizationManager.Get("editorwindow.generatetexture.success"),
                    LocalizationManager.Get("editorwindow.generatetexture.success.yes"),
                    LocalizationManager.Get("editorwindow.generatetexture.success.no")
                );

                if (string.IsNullOrEmpty(savedPath)) return;

                if (confirm)
                {
                    var textureReplacer = new GameObject();
                    textureReplacer.transform.SetParent(colorChangerComponent.GetComponentInParent<VRC_AvatarDescriptor>().gameObject.transform);
                    var component = textureReplacer.AddComponent<PukosTextureReplacer>();
                    
                    component.originalTexture = colorChangerComponent.targetTexture;
                    component.targetTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(savedPath);
                }

                UnityUtils.SelectAssetAtPath(savedPath);
            }
            catch (Exception ex)
            {
                LogUtils.LogError(LocalizationManager.Get("editorwindow.generatetexture.failed", colorChangerComponent.name, ex.ToString()));
            }
            finally
            {
                DestroyImmediate(originalTexture);
                DestroyImmediate(newTexture);
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
