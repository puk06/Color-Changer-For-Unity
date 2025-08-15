using nadena.dev.ndmf;
using net.puk06.ColorChanger.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            Dictionary<Texture2D, Texture2D> processedDictionary = new();

            var components = avatar.GetComponentsInChildren<ColorChangerForUnity>();
            if (components == null || components.Length == 0) return;

            // 中身が有効なコンポーネントだけ取り出す。Enabledもここでチェック。
            var enabledComponents = components.Where(x => x != null && x.Enabled && x.targetTexture != null);
            if (enabledComponents == null || !enabledComponents.Any()) return;

            // このアバター配下の全てのRendererが使っている全てのテクスチャのハッシュ一覧
            var avatarTexturesHashSet = TextureUtils.GetAvatarTexturesHashSet(avatar);
            if (avatarTexturesHashSet == null || !avatarTexturesHashSet.Any()) return;

            var avatarComponents = enabledComponents
                .Where(c => avatarTexturesHashSet.Contains(c.targetTexture));
            if (avatarComponents == null || !avatarComponents.Any()) return;

            foreach (var component in avatarComponents)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                LogUtils.Log($"Texture Processing Start: '{component.name}'");

                try
                {
                    Texture2D originalTexture = GetRawTexture(component.targetTexture);
                    Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);

                    TextureUtils.ProcessTexture(originalTexture, newTexture, component);

                    AssetDatabase.AddObjectToAsset(newTexture, buildContext.AssetContainer);
                    processedDictionary.Add(component.targetTexture, newTexture);

                    Object.DestroyImmediate(originalTexture);
                    stopwatch.Stop();

                    LogUtils.Log($"Texture Processing Done: '{component.name}' | {stopwatch.ElapsedMilliseconds} ms");
                }
                catch (Exception ex)
                {
                    LogUtils.LogError($"Texture Processing Error: '{component.name}' | {stopwatch.ElapsedMilliseconds} ms\n" + ex);
                }
            }

            Renderer[] renderers = avatar.GetComponentsInChildren<Renderer>();
            ReplaceTextures(renderers, processedDictionary);

            foreach (var component in avatar.GetComponentsInChildren<ColorChangerForUnity>())
            {
                Object.DestroyImmediate(component);
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
                            if (!processedTextureDictionary.TryGetValue(texture as Texture2D, out Texture2D newTexture)) return;
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

        private Texture2D GetRawTexture(Texture2D source)
        {
            RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default);
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
