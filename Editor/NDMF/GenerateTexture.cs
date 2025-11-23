#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using nadena.dev.ndmf;
using net.puk06.ColorChanger.Localization;
using net.puk06.ColorChanger.Models;
using net.puk06.ColorChanger.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace net.puk06.ColorChanger.NDMF
{
    public class GenerateColorChangedTexture : Pass<GenerateColorChangedTexture>
    {
        protected override void Execute(BuildContext buildContext)
        {
            var avatar = buildContext.AvatarRootObject;

            var components = avatar.GetComponentsInChildren<ColorChangerForUnity>(true)
#if USE_TEXTRANSTOOL
                .Where(component => !component.GetComponent<rs64.TexTransTool.MultiLayerImage.ExternalToolAsLayer>())
                .ToArray()
#endif
                ;
            if (components == null || !components.Any()) return;

            try
            {
                // 中身が有効なコンポーネントだけ取り出す。Enabledもここでチェック。
                var enabledComponents = components.Where(x => ColorChangerUtils.IsEnabled(x));
                if (!enabledComponents.Any()) return;
                
                var enabledInternalComponentsValues = new List<InternalColorChangerValues>();
                foreach (var component in enabledComponents)
                {
                    enabledInternalComponentsValues.Add(new InternalColorChangerValues(component, component.ComponentTexture, true));
                    foreach (var otherTexture in component.settingsInheritedTextures)
                    {
                        if (otherTexture == null) continue;
                        enabledInternalComponentsValues.Add(new InternalColorChangerValues(component, otherTexture, false));
                    }
                }

                // このアバター配下の全てのRendererが使っている全てのテクスチャのハッシュ一覧
                var avatarRenderers = TextureUtils.GetRenderers(avatar);
                var avatarTexturesHashSet = TextureUtils.GetRenderersTexturesHashSet(avatarRenderers);
                if (avatarTexturesHashSet == null || !avatarTexturesHashSet.Any()) return;

                var avatarComponents = enabledInternalComponentsValues
                    .Where(c => avatarTexturesHashSet.Contains(c.targetTexture!));
                if (!avatarComponents.Any()) return;

                // ターゲットテクスチャごとに分ける。これは複数同じテクスチャがあった時対策
                var groupedComponents = avatarComponents
                    .GroupBy(c => c.targetTexture);

                Dictionary<Texture2D, Texture2D> processedDictionary = new();

                foreach (var groupedComponent in groupedComponents)
                {
                    var firstComponent = groupedComponent.FirstOrDefault();
                    if (firstComponent == null) continue;

                    if (groupedComponent.Count() >= 2)
                    {
                        LogUtils.LogWarning($"Duplicate targetTexture detected: '{groupedComponent.Key!.name}' (using settings from '{firstComponent.parentComponent.name}')");
                    }

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    LogUtils.Log($"Texture Processing Start: '{firstComponent.parentComponent.name}'");

                    try
                    {
                        Texture2D newTexture2D = TextureUtils.GetProcessedTexture(firstComponent.targetTexture!, firstComponent.parentComponent, firstComponent.useMask);

                        AssetDatabase.AddObjectToAsset(newTexture2D, buildContext.AssetContainer);
                        processedDictionary.Add(firstComponent.targetTexture!, newTexture2D);

                        stopwatch.Stop();

                        LogUtils.Log($"Texture Processing Done: '{firstComponent.parentComponent.name}' | {stopwatch.ElapsedMilliseconds} ms");

                        // NDMF Console Log
                        ErrorReport.ReportError(ColorChangerLocalizer.GetLocalizer(), ErrorSeverity.Information, "colorchanger.process.success", firstComponent.parentComponent, firstComponent.targetTexture!.name, stopwatch.ElapsedMilliseconds.ToString());
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();

                        LogUtils.LogError($"Texture Processing Error: '{firstComponent.parentComponent.name}' | {stopwatch.ElapsedMilliseconds} ms\n{ex}");

                        // NDMF Console Log
                        ErrorReport.ReportError(ColorChangerLocalizer.GetLocalizer(), ErrorSeverity.Error, "colorchanger.process.error", firstComponent.parentComponent, firstComponent.targetTexture!.name, stopwatch.ElapsedMilliseconds.ToString());
                    }
                }

                Renderer[] renderers = avatar.GetComponentsInChildren<Renderer>();
                ReplaceTextures(renderers, processedDictionary);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occured while processing avatar: '{avatar.name}'\n{ex}");
            }
        }

        private void ReplaceTextures(Renderer[] renderers, Dictionary<Texture2D, Texture2D> processedTextureDictionary)
        {
            foreach (var renderer in renderers)
            {
                Material[] materials = renderer.sharedMaterials;
                Material[] newMaterials = new Material[materials.Length];

                var materialsToChange = processedTextureDictionary.Keys
                    .SelectMany(tex => TextureUtils.FindMaterialsWithTexture(materials, tex))
                    .ToList();

                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == null) continue;

                    if (materialsToChange.Contains(materials[i]))
                    {
                        newMaterials[i] = new Material(materials[i]);

                        MaterialUtils.ForEachTex(newMaterials[i], (texture, propName) =>
                        {
                            if (!processedTextureDictionary.TryGetValue((Texture2D)texture, out Texture2D newTexture)) return;
                            newMaterials[i].SetTexture(propName, newTexture);
                        });
                    }
                    else
                    {
                        newMaterials[i] = materials[i];
                    }
                }

                renderer.sharedMaterials = newMaterials;
            }
        }
    }
}
