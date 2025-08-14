using net.puk06.ColorChanger.Utils;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace net.puk06.ColorChanger {
    [CustomEditor(typeof(ColorChangerForUnity))]
    [CanEditMultipleObjects]
    public class ColorChanger : Editor
    {
        private Texture2D logoTexture;
        private Texture2D componentIcon;

        private SerializedProperty enabledButtonProp;
        private SerializedProperty targetTextureProp;
        private SerializedProperty previousColorProp;
        private SerializedProperty newColorProp;

        private SerializedProperty balanceModeConfigProp;
        private SerializedProperty advancedColorConfigProp;

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
            logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/net.puk06.color-changer/Editor/Assets/ComponentLogo.png");
            componentIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/net.puk06.color-changer/Editor/Assets/ComponentIcon.png");

            enabledButtonProp = serializedObject.FindProperty("Enabled");
            targetTextureProp = serializedObject.FindProperty("targetTexture");
            previousColorProp = serializedObject.FindProperty("previousColor");
            newColorProp = serializedObject.FindProperty("newColor");

            balanceModeConfigProp = serializedObject.FindProperty("balanceModeConfiguration");
            advancedColorConfigProp = serializedObject.FindProperty("advancedColorConfiguration");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
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

            ColorChangerForUnity comp = (ColorChangerForUnity)target;
            if (componentIcon != null) EditorGUIUtility.SetIconForObject(comp, componentIcon);

            // スクリプト設定画面
            DrawColorChangerSettingsGUI(comp);

            // テクスチャ設定画面
            DrawTextureSettingsGUI(comp);

            // 色設定画面
            DrawColorSettingsGUI(comp);

            // バランスモード画面
            DrawBalanceModeSettingsGUI(comp);

            // 色の追加設定画面
            DrawAdvancedColorModeSettingsGUI(comp);

            EditorGUILayout.Space(10);

            // テクスチャ作成ボタン
            DrawTextureOutputGUI(comp);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawColorChangerSettingsGUI(ColorChangerForUnity colorChangerComponent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.foldout);
            titleStyle.fontSize = 13;
            titleStyle.fontStyle = FontStyle.Bold;

            // インデントリセット
            EditorGUI.indentLevel = 1;

            showColorChangerSettings = EditorGUILayout.Foldout(showColorChangerSettings, "スクリプト設定", true, titleStyle);
            if (!showColorChangerSettings)
            {
                EditorGUI.indentLevel = 0;
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUI.indentLevel++;

            enabledButtonProp.boolValue = EditorGUILayout.Toggle("スクリプトの有効化", enabledButtonProp.boolValue);

            EditorGUI.indentLevel = 0;
            EditorGUILayout.EndVertical();
        }

        private void DrawTextureSettingsGUI(ColorChangerForUnity colorChangerComponent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.foldout);
            titleStyle.fontSize = 13;
            titleStyle.fontStyle = FontStyle.Bold;

            EditorGUI.indentLevel = 1;

            showTextureSettings = EditorGUILayout.Foldout(
                showTextureSettings,
                "テクスチャ設定",
                true,
                titleStyle
            );

            if (showTextureSettings)
            {
                targetTextureProp.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField("適用したいテクスチャ", (Texture2D)targetTextureProp.objectReferenceValue, typeof(Texture2D), true);

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

        private void DrawColorSettingsGUI(ColorChangerForUnity colorChangerComponent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.foldout);
            titleStyle.fontSize = 13;
            titleStyle.fontStyle = FontStyle.Bold;

            EditorGUI.indentLevel = 1;

            showColorSettings = EditorGUILayout.Foldout(
                showColorSettings,
                "色設定",
                true,
                titleStyle
            );

            if (showColorSettings)
            {
                EditorGUI.indentLevel = 2;
                previousColorProp.colorValue = EditorGUILayout.ColorField("変更前の色", previousColorProp.colorValue);
                newColorProp.colorValue = EditorGUILayout.ColorField("変更後の色", newColorProp.colorValue);
                EditorGUI.indentLevel = 1;
            }

            EditorGUI.indentLevel = 0;

            EditorGUILayout.EndVertical();
        }

        private void DrawBalanceModeSettingsGUI(ColorChangerForUnity colorChangerComponent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.foldout);
            titleStyle.fontSize = 13;
            titleStyle.fontStyle = FontStyle.Bold;

            EditorGUI.indentLevel = 1;

            showBalanceModeSettings = EditorGUILayout.Foldout(
                showBalanceModeSettings,
                "バランスモード設定",
                true,
                titleStyle
            );

            if (showBalanceModeSettings)
            {
                EditorGUI.indentLevel = 2;

                SerializedProperty modeVersionProp = balanceModeConfigProp.FindPropertyRelative("ModeVersion");
                modeVersionProp.intValue = (int)(BalanceModeSettings)EditorGUILayout.EnumPopup(
                    "バランスモード", (BalanceModeSettings)modeVersionProp.intValue
                );

                GUIStyle subTitleStyle = new GUIStyle(EditorStyles.foldout);
                subTitleStyle.fontSize = 12;
                subTitleStyle.fontStyle = FontStyle.Bold;

                showBalanceModeV1Settings = EditorGUILayout.Foldout(
                    showBalanceModeV1Settings,
                    "バランスモードV1",
                    true,
                    subTitleStyle
                );

                if (showBalanceModeV1Settings)
                {
                    EditorGUI.indentLevel = 3;
                    SerializedProperty v1WeightProp = balanceModeConfigProp.FindPropertyRelative("V1Weight");
                    SerializedProperty v1MinValueProp = balanceModeConfigProp.FindPropertyRelative("V1MinimumValue");
                    v1WeightProp.floatValue = EditorGUILayout.FloatField("変化率グラフの重み", v1WeightProp.floatValue);
                    v1MinValueProp.floatValue = EditorGUILayout.FloatField("変化率グラフの最低値", v1MinValueProp.floatValue);
                    EditorGUI.indentLevel = 2;
                }

                showBalanceModeV2Settings = EditorGUILayout.Foldout(
                    showBalanceModeV2Settings,
                    "バランスモードV2",
                    true,
                    subTitleStyle
                );

                if (showBalanceModeV2Settings)
                {
                    EditorGUI.indentLevel = 3;
                    SerializedProperty v2RadiusProp = balanceModeConfigProp.FindPropertyRelative("V2Radius");
                    SerializedProperty v2WeightProp = balanceModeConfigProp.FindPropertyRelative("V2Weight");
                    SerializedProperty v2MinValueProp = balanceModeConfigProp.FindPropertyRelative("V2MinimumValue");
                    SerializedProperty v2IncludeOutsideProp = balanceModeConfigProp.FindPropertyRelative("V2IncludeOutside");

                    v2RadiusProp.floatValue = EditorGUILayout.FloatField("球の半径の最大値", v2RadiusProp.floatValue);
                    v2WeightProp.floatValue = EditorGUILayout.FloatField("変化率グラフの重み", v2WeightProp.floatValue);
                    v2MinValueProp.floatValue = EditorGUILayout.FloatField("変化率グラフの最低値", v2MinValueProp.floatValue);
                    v2IncludeOutsideProp.boolValue = EditorGUILayout.Toggle("範囲外にも最低値を適用する", v2IncludeOutsideProp.boolValue);
                    EditorGUI.indentLevel = 2;
                }

                showBalanceModeV3Settings = EditorGUILayout.Foldout(
                    showBalanceModeV3Settings,
                    "バランスモードV3",
                    true,
                    subTitleStyle
                );

                if (showBalanceModeV3Settings)
                {
                    EditorGUI.indentLevel = 3;
                    SerializedProperty v3GradientProp = balanceModeConfigProp.FindPropertyRelative("V3GradientColor");
                    SerializedProperty v3GradientResolutionProp = balanceModeConfigProp.FindPropertyRelative("V3GradientPreviewResolution");

                    v3GradientProp.gradientValue = EditorGUILayout.GradientField("グラデーション", v3GradientProp.gradientValue);
                    v3GradientResolutionProp.intValue = EditorGUILayout.IntField("プレビュー解像度", v3GradientResolutionProp.intValue);
                    EditorGUI.indentLevel = 2;
                }
            }

            EditorGUI.indentLevel = 1;

            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedColorModeSettingsGUI(ColorChangerForUnity colorChangerComponent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.foldout);
            titleStyle.fontSize = 13;
            titleStyle.fontStyle = FontStyle.Bold;

            // インデントリセット
            EditorGUI.indentLevel = 1;

            showAdvancedColorSettings = EditorGUILayout.Foldout(showAdvancedColorSettings, "色の追加設定", true, titleStyle);
            if (!showAdvancedColorSettings)
            {
                EditorGUI.indentLevel = 0;
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUI.indentLevel++;

            SerializedProperty enabledProp = advancedColorConfigProp.FindPropertyRelative("Enabled");
            SerializedProperty brightnessProp = advancedColorConfigProp.FindPropertyRelative("Brightness");
            SerializedProperty contrastProp = advancedColorConfigProp.FindPropertyRelative("Contrast");
            SerializedProperty gammaProp = advancedColorConfigProp.FindPropertyRelative("Gamma");
            SerializedProperty exposureProp = advancedColorConfigProp.FindPropertyRelative("Exposure");
            SerializedProperty transparencyProp = advancedColorConfigProp.FindPropertyRelative("Transparency");

            enabledProp.boolValue = EditorGUILayout.Toggle("有効", enabledProp.boolValue);
            brightnessProp.floatValue = EditorGUILayout.FloatField("明るさ", brightnessProp.floatValue);
            contrastProp.floatValue = EditorGUILayout.FloatField("コントラスト", contrastProp.floatValue);
            gammaProp.floatValue = EditorGUILayout.FloatField("ガンマ", gammaProp.floatValue);
            exposureProp.floatValue = EditorGUILayout.FloatField("露出", exposureProp.floatValue);
            transparencyProp.floatValue = EditorGUILayout.FloatField("透明度", transparencyProp.floatValue);

            EditorGUI.indentLevel = 0;
            EditorGUILayout.EndVertical();
        }

        private void DrawTextureOutputGUI(ColorChangerForUnity colorChangerComponent)
        {
            EditorGUI.indentLevel = 0;
            EditorGUILayout.HelpBox("テクスチャはビルド時に自動で非破壊で作成、適用されます。\nテクスチャ画像の細かな修正が必要な場合はテクスチャを出力して各自で編集してください。", MessageType.Warning);
            if (GUILayout.Button("テクスチャ出力(非推奨)", GUILayout.ExpandWidth(true)))
            {
                GenerateTexture(colorChangerComponent);
            }
        }

        private void GenerateTexture(ColorChangerForUnity colorChangerComponent)
        {
            if (colorChangerComponent.targetTexture == null)
            {
                LogUtils.LogError("ターゲットテクスチャが選択されていません。");
                return;
            }

            Texture2D originalTexture = ConvertToNonCompressed(colorChangerComponent.targetTexture);
            Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false, true);

            TextureUtils.ProcessTexture(originalTexture, newTexture, colorChangerComponent);

            string savedPath = SaveTextureWithUniqueName(colorChangerComponent.targetTexture, newTexture);

            bool confirm = EditorUtility.DisplayDialog(
                "確認",
                "テクスチャの作成が完了しました。\nこのテクスチャを使用しているマテリアルを、現在のシーン内で更新しますか？",
                "はい",
                "いいえ"
            );

            if (string.IsNullOrEmpty(savedPath)) return;

            if (confirm)
            {
                TextureUtils.ReplaceTextureInSceneObjects(colorChangerComponent.targetTexture, savedPath);
            }

            UnityUtils.SelectAssetAtPath(savedPath);
        }

        private string SaveTextureWithUniqueName(Texture2D originalTexture, Texture2D newTexture)
        {
            string originalPath = AssetDatabase.GetAssetPath(originalTexture);
            if (string.IsNullOrEmpty(originalPath))
            {
                LogUtils.LogError("元テクスチャのパスが取得できません");
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
                LogUtils.LogError("PNGデータのエンコードに失敗しました");
                return string.Empty;
            }

            File.WriteAllBytes(savePath, pngData);
            LogUtils.Log($"テクスチャを保存しました: {savePath}");

            AssetDatabase.ImportAsset(savePath);
            AssetDatabase.Refresh();

            return savePath;
        }

        private Texture2D ConvertToNonCompressed(Texture2D source)
        {
            RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default);
            Graphics.Blit(source, rt);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D readableTexture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, true);

            readableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readableTexture.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return readableTexture;
        }
    }
}
