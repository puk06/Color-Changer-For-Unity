using nadena.dev.ndmf.preview;
using net.puk06.ColorChanger.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace net.puk06.ColorChanger.NDMF
{
    internal class NDMFPreview : IRenderFilter
    {
        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            var avatars = context.GetAvatarRoots();

            var resultSet = new List<RenderGroup>();
            foreach (var avatar in avatars)
            {
                try
                {
                    // アバター内にある全部のコンポーネント
                    var components = avatar.GetComponentsInChildren<ColorChangerForUnity>();
                    if (components == null) continue;

                    // その中で参照されてる全てのテクスチャ
                    var targetTextures = components
                        .Select(c => c.targetTexture)
                        .Where(t => t != null)
                        .Distinct()
                        .ToArray();

                    // アバター内の全てのレンダラー
                    var avatarRenderers = avatar.GetComponentsInChildren<Renderer>()?
                        .Where(r => r is MeshRenderer or SkinnedMeshRenderer)
                        .GroupBy(r => r.gameObject);
                    if (avatarRenderers == null) continue;

                    var rendererList = new List<Renderer>();

                    foreach (var avatarRenderer in avatarRenderers)
                    {
                        var firstComponent = avatarRenderer.FirstOrDefault();
                        if (firstComponent == null) continue;

                        if (avatarRenderer.Count() >= 2)
                        {
                            LogUtils.LogWarning($"Duplicate Renderer GameObject detected: '{avatarRenderer.Key.name}' (using settings from '{firstComponent.GetType()}' component)");
                        }

                        var materials = firstComponent.sharedMaterials;
                        if (materials == null) continue;

                        if (!materials.Any(material => targetTextures.Any(targetTexture => targetTexture != null && MaterialUtils.AnyTex(material, targetTexture))))
                            rendererList.Add(firstComponent);
                    }

                    foreach (var component in components)
                    {
                        if (component != null)
                            context.Observe(component, c => c.targetTexture);
                    }

                    if (rendererList.Count > 0)
                        resultSet.Add(RenderGroup.For(rendererList));
                }
                catch (Exception ex)
                {
                    LogUtils.LogError($"Failed to add renderer for avatar '{avatar.name}'.\n{ex}");
                }
            }

            return resultSet.ToImmutableList();
        }

        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            try
            {
                // シーン内の全てのColorChangerForUnityコンポーネントを出してくる。
                var components = context.GetComponentsByType<ColorChangerForUnity>();

                // 中身が有効なコンポーネントだけ取り出す。Enabledもここでチェック。
                var enabledComponents = components.Where(x => x.Enabled && x.targetTexture != null);

                // 変更される予定のテクスチャの配列
                var targetTextures = enabledComponents
                    .Select(c => c.targetTexture)
                    .Distinct()
                    .ToArray();

                // 元のテクスチャ、処理されたテクスチャのDictionary
                var processedTextures = new Dictionary<Texture2D, RenderTexture>();

                // ターゲットテクスチャごとに分ける。これは複数同じテクスチャがあった時対策
                var groupedComponents = enabledComponents
                    .GroupBy(c => c.targetTexture);

                foreach (var groupedComponent in groupedComponents)
                {
                    var firstComponent = groupedComponent.FirstOrDefault();
                    if (firstComponent == null) continue;

                    if (groupedComponent.Count() >= 2)
                    {
                        LogUtils.LogWarning($"Duplicate targetTexture detected: '{groupedComponent.Key.name}' (using settings from '{firstComponent.gameObject.name}')");
                    }

                    // テクスチャを作る
                    var processedTexture = ComputeTextureOverrides(firstComponent);
                    if (processedTexture == null) continue;

                    processedTextures.Add(groupedComponent.Key, processedTexture);
                }

                // テクスチャが含まれているマテリアルすべてを探す。
                var relevantMaterials = group.Renderers
                    .SelectMany(r => r.sharedMaterials)
                    .Where(material => targetTextures.Any(tex => MaterialUtils.AnyTex(material, tex)))
                    .Distinct()
                    .ToList();

                // テクスチャが含まれているマテリアル全てを複製 + 新しいテクスチャに置き換える
                var processedMaterials = relevantMaterials
                    .ToDictionary(
                        material => material,
                        material => ProcessMaterial(material, processedTextures)
                    );

                components.ForEach(component => context.Observe(component));

                // 変換前、変換後のマテリアルテクスチャをまとめたものを渡してあげる。
                return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(processedMaterials));
            }
            catch (Exception ex)
            {
                LogUtils.LogError($"Failed to instantiate.\n{ex.Message}");
                return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(null));
            }
        }

        private Material ProcessMaterial(Material material, Dictionary<Texture2D, RenderTexture> processedTextures)
        {
            var newMat = new Material(material);
            MaterialUtils.ForEachTex(newMat, (tex, propName) =>
            {
                if (processedTextures.TryGetValue(tex as Texture2D, out var renderTex))
                    newMat.SetTexture(propName, renderTex);
            });
            return newMat;
        }

        private static RenderTexture ComputeTextureOverrides(ColorChangerForUnity component)
        {
            if (component.targetTexture == null) return null;
            RenderTexture rawTexture = GetRawRenderTexture(component.targetTexture);
            RenderTexture newTex = TextureUtils.GenerateRenderTexture(component);

            TextureUtils.ProcessTexture(rawTexture, newTex, component);

            if (RenderTexture.active == rawTexture) RenderTexture.active = null;
            rawTexture.DiscardContents();
            Object.DestroyImmediate(rawTexture);

            return newTex;
        }

        private static RenderTexture GetRawRenderTexture(Texture2D source)
        {
            var renderTexture = new RenderTexture(source.width, source.height, 0);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            Graphics.Blit(source, renderTexture);
            return renderTexture;
        }

        private class TextureReplacerNode : IRenderFilterNode, IDisposable
        {
            private readonly Dictionary<Material, Material> _materialDictionary;

            public RenderAspects WhatChanged { get; private set; } = RenderAspects.Texture & RenderAspects.Material;

            public TextureReplacerNode(Dictionary<Material, Material> materialDictionary)
            {
                if (materialDictionary == null) return;
                _materialDictionary = materialDictionary;
            }

            public void OnFrame(Renderer original, Renderer proxy)
            {
                try
                {
                    if (proxy == null || proxy.sharedMaterials == null || _materialDictionary == null || _materialDictionary.Count == 0) return;

                    var newMaterials = new Material[proxy.sharedMaterials.Length];
                    for (int i = 0; i < proxy.sharedMaterials.Length; i++)
                    {
                        var material = proxy.sharedMaterials[i];
                        if (material == null) continue;

                        if (_materialDictionary.TryGetValue(material, out var newMaterial))
                        {
                            newMaterials[i] = newMaterial;
                        }
                        else
                        {
                            newMaterials[i] = material;
                        }
                    }

                    proxy.sharedMaterials = newMaterials;
                }
                catch (Exception ex)
                {
                    LogUtils.LogError("Error occurred while rendering proxy.\n" + ex);
                }
            }

            public void Dispose()
            {
                foreach (var material in _materialDictionary.Values)
                {
                    MaterialUtils.ForEachTex(material, (texture, propName) =>
                    {
                        if (texture is not RenderTexture rt) return;

                        rt.DiscardContents();
                        Object.DestroyImmediate(rt);
                    });

                    Object.DestroyImmediate(material);
                }

                _materialDictionary.Clear();
            }
        }
    }
}
