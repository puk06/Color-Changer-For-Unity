using UnityEditor;
using UnityEngine;
using net.puk06.ColorChanger.ImageProcessing;
using net.puk06.ColorChanger.Models;
using net.puk06.ColorChanger.Utils;
using System.IO;

namespace net.puk06.ColorChanger {
    [CustomEditor(typeof(ColorChangerForUnity))]
    public class ColorChanger : Editor
    {
        private Texture2D logoTexture;

        private bool showTextureSettings = true;
        private bool showColorSettings = false;
        private bool showBalanceModeSettings = false;
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
            logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/net.puk06.color-changer/Editor/Assets/logo.png");
        }

        public override void OnInspectorGUI()
        {
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

            // テクスチャ設定画面
            DrawTextureSettingsGUI(comp);

            // 色設定画面
            DrawColorSettingsGUI(comp);

            // バランスモード画面
            DrawBalanceModeSettingsGUI(comp);

            // 色の追加設定画面
            DrawAdvancedColorModeSettingsGUI(comp);

            // テクスチャ作成ボタン
            DrawTextureOutputGUI(comp);

            // 変更があれば保存
            if (GUI.changed)
            {
                EditorUtility.SetDirty(comp);
            }
        }

        private void DrawTextureSettingsGUI(ColorChangerForUnity colorChangerComponent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.foldout);
            titleStyle.fontSize = 13;
            titleStyle.fontStyle = FontStyle.Bold;

            // インデントリセット
            int prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 1;

            showTextureSettings = EditorGUILayout.Foldout(showTextureSettings, "テクスチャ設定", titleStyle);

            if (showTextureSettings)
            {
                colorChangerComponent.targetTexture = (Texture2D)EditorGUILayout.ObjectField("適用したいテクスチャ", colorChangerComponent.targetTexture, typeof(Texture2D), true);

                if (colorChangerComponent.targetTexture != null)
                {
                    float displayWidth = EditorGUIUtility.currentViewWidth - 40;
                    float aspect = (float)colorChangerComponent.targetTexture.height / colorChangerComponent.targetTexture.width;
                    float displayHeight = displayWidth * aspect;

                    Rect rect = GUILayoutUtility.GetRect(displayWidth, displayHeight, GUILayout.ExpandWidth(false));
                    rect.x = (EditorGUIUtility.currentViewWidth - rect.width) / 2;

                    GUI.DrawTexture(rect, colorChangerComponent.targetTexture, ScaleMode.ScaleToFit);
                }
            }

            // インデント復元
            EditorGUI.indentLevel = prevIndent;

            EditorGUILayout.EndVertical();
        }

        private void DrawColorSettingsGUI(ColorChangerForUnity colorChangerComponent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.foldout);
            titleStyle.fontSize = 13;
            titleStyle.fontStyle = FontStyle.Bold;

            // インデントリセット
            int prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 1;

            showColorSettings = EditorGUILayout.Foldout(showColorSettings, "色設定", titleStyle);

            if (showColorSettings)
            {
                colorChangerComponent.previousColor = EditorGUILayout.ColorField("変更前の色", colorChangerComponent.previousColor);
                colorChangerComponent.newColor = EditorGUILayout.ColorField("変更後の色", colorChangerComponent.newColor);
            }

            // インデント復元
            EditorGUI.indentLevel = prevIndent;

            EditorGUILayout.EndVertical();
        }

        private void DrawBalanceModeSettingsGUI(ColorChangerForUnity colorChangerComponent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.foldout);
            titleStyle.fontSize = 13;
            titleStyle.fontStyle = FontStyle.Bold;

            // インデントリセット
            int prevIndent = EditorGUI.indentLevel;
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
                colorChangerComponent.balanceModeConfiguration.ModeVersion = (int)(BalanceModeSettings)EditorGUILayout.EnumPopup(
                    "バランスモード", (BalanceModeSettings)colorChangerComponent.balanceModeConfiguration.ModeVersion
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

                EditorGUI.indentLevel = 3;
                if (showBalanceModeV1Settings)
                {
                    colorChangerComponent.balanceModeConfiguration.V1Weight = EditorGUILayout.FloatField("変化率グラフの重み", colorChangerComponent.balanceModeConfiguration.V1Weight);
                    colorChangerComponent.balanceModeConfiguration.V1MinimumValue = EditorGUILayout.FloatField("変化率グラフの最低値", colorChangerComponent.balanceModeConfiguration.V1MinimumValue);
                }
                EditorGUI.indentLevel = 2;

                showBalanceModeV2Settings = EditorGUILayout.Foldout(
                    showBalanceModeV2Settings,
                    "バランスモードV2",
                    true,
                    subTitleStyle
                );

                EditorGUI.indentLevel = 3;
                if (showBalanceModeV2Settings)
                {
                    colorChangerComponent.balanceModeConfiguration.V2Radius = EditorGUILayout.FloatField("球の半径の最大値", colorChangerComponent.balanceModeConfiguration.V2Radius);
                    colorChangerComponent.balanceModeConfiguration.V2Weight = EditorGUILayout.FloatField("変化率グラフの重み", colorChangerComponent.balanceModeConfiguration.V2Weight);
                    colorChangerComponent.balanceModeConfiguration.V2MinimumValue = EditorGUILayout.FloatField("変化率グラフの最低値", colorChangerComponent.balanceModeConfiguration.V2MinimumValue);
                    colorChangerComponent.balanceModeConfiguration.V2IncludeOutside = EditorGUILayout.Toggle("範囲外にも最低値を適用する", colorChangerComponent.balanceModeConfiguration.V2IncludeOutside);
                }
                EditorGUI.indentLevel = 2;

                showBalanceModeV3Settings = EditorGUILayout.Foldout(
                    showBalanceModeV3Settings,
                    "バランスモードV3",
                    true,
                    subTitleStyle
                );

                EditorGUI.indentLevel = 3;
                if (showBalanceModeV3Settings)
                {
                    colorChangerComponent.balanceModeConfiguration.V3GradientColor = EditorGUILayout.GradientField("グラデーション", colorChangerComponent.balanceModeConfiguration.V3GradientColor);
                }
                EditorGUI.indentLevel = 2;
            }

            // インデント復元
            EditorGUI.indentLevel = prevIndent;

            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedColorModeSettingsGUI(ColorChangerForUnity colorChangerComponent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.foldout);
            titleStyle.fontSize = 13;
            titleStyle.fontStyle = FontStyle.Bold;

            // インデントリセット
            int prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 1;

            showAdvancedColorSettings = EditorGUILayout.Foldout(
                showAdvancedColorSettings,
                "色の追加設定",
                true,
                titleStyle
            );

            if (showAdvancedColorSettings)
            {
                EditorGUI.indentLevel = 2;

                colorChangerComponent.advancedColorConfiguration.Enabled = EditorGUILayout.Toggle("有効化", colorChangerComponent.advancedColorConfiguration.Enabled);
                colorChangerComponent.advancedColorConfiguration.Brightness = EditorGUILayout.FloatField("明度", colorChangerComponent.advancedColorConfiguration.Brightness);
                colorChangerComponent.advancedColorConfiguration.Contrast = EditorGUILayout.FloatField("コントラスト", colorChangerComponent.advancedColorConfiguration.Contrast);
                colorChangerComponent.advancedColorConfiguration.Gamma = EditorGUILayout.FloatField("ガンマ補正", colorChangerComponent.advancedColorConfiguration.Gamma);
                colorChangerComponent.advancedColorConfiguration.Exposure = EditorGUILayout.FloatField("露出", colorChangerComponent.advancedColorConfiguration.Exposure);
                colorChangerComponent.advancedColorConfiguration.Transparency = EditorGUILayout.FloatField("透明度", colorChangerComponent.advancedColorConfiguration.Transparency);

                // インデント復元
                EditorGUI.indentLevel = prevIndent;
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawTextureOutputGUI(ColorChangerForUnity colorChangerComponent)
        {
            if (GUILayout.Button("テクスチャ出力", GUILayout.ExpandWidth(true)))
            {
                GenerateTexture(colorChangerComponent);
            }
        }

        private void GenerateTexture(ColorChangerForUnity colorChangerComponent)
        {
            if (colorChangerComponent.targetTexture == null)
            {
                Debug.LogError("ターゲットテクスチャが選択されていません。");
                return;
            }

            Texture2D originalTexture = ConvertToNonCompressed(colorChangerComponent.targetTexture);
            Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);

            ColorDifference colorDifference = new ColorDifference(colorChangerComponent.previousColor, colorChangerComponent.newColor);

            ImageProcessor imageProcessor = new ImageProcessor(colorDifference);

            if (colorChangerComponent.balanceModeConfiguration.ModeVersion != 0)
            {
                imageProcessor.SetBalanceSettings(colorChangerComponent.balanceModeConfiguration);
            }

            if (colorChangerComponent.advancedColorConfiguration.Enabled)
            {
                imageProcessor.SetColorSettings(colorChangerComponent.advancedColorConfiguration);
            }

            imageProcessor.ProcessAllPixels(originalTexture, newTexture);

            string savedPath = SaveTextureWithUniqueName(colorChangerComponent.targetTexture, newTexture);
            
            if (string.IsNullOrEmpty(savedPath)) return;
            TextureReplacer.ReplaceTextureInMaterials(colorChangerComponent.targetTexture, savedPath);
        }

        private string SaveTextureWithUniqueName(Texture2D originalTexture, Texture2D newTexture)
        {
            string originalPath = AssetDatabase.GetAssetPath(originalTexture);
            if (string.IsNullOrEmpty(originalPath))
            {
                Debug.LogError("元テクスチャのパスが取得できません");
                return string.Empty;
            }

            string directory = Path.GetDirectoryName(originalPath);
            string originalFileName = Path.GetFileNameWithoutExtension(originalPath);
            string extension = ".png";

            int index = 1;
            string savePath;
            do
            {
                string fileName = $"{originalFileName}_{index}{extension}";
                savePath = Path.Combine(directory, fileName);
                index++;
            } while (File.Exists(savePath));

            byte[] pngData = newTexture.EncodeToPNG();
            if (pngData == null)
            {
                Debug.LogError("PNGデータのエンコードに失敗しました");
                return string.Empty;
            }

            File.WriteAllBytes(savePath, pngData);
            Debug.Log($"テクスチャを保存しました: {savePath}");

            AssetDatabase.ImportAsset(savePath);
            AssetDatabase.Refresh();

            return savePath;
        }

        private Texture2D ConvertToNonCompressed(Texture2D source)
        {
            RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(source, rt);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D readableTexture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);

            readableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readableTexture.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return readableTexture;
        }
    }
}
