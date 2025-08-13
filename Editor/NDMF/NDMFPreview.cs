using nadena.dev.ndmf.preview;
using net.puk06.ColorChanger.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
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

                    // その中で参照されてる全てのテクスチャ (重複対策してあります)
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

                        if (materials.Any(material => targetTextures.Any(targetTexture => targetTexture != null && MaterialUtils.AnyTex(material, targetTexture))))
                        {
                            rendererList.Add(firstComponent);
                        }
                    }

                    foreach (var component in components)
                    {
                        context.Observe(component, c => c.targetTexture);
                    }

                    //レンダラーリストは、コンポーネントによってアバター内のどれかのマテリアルテクスチャが参照されているレンダラーのリスト
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
                var firstRenderer = group.Renderers.FirstOrDefault();
                if (firstRenderer == null) return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(null));

                var avatar = context.GetAvatarRoot(firstRenderer.gameObject);
                if (avatar == null) return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(null));

                // アバター配下にあるColorChangerコンポーネントの配列
                var components = avatar.GetComponentsInChildren<ColorChangerForUnity>();
                if (components == null || components.Length == 0) return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(null));
                
                // 早期リターンで監視対象から外れるのを防ぐため
                foreach (var component in components)
                {
                    context.Observe(component);
                }

                // 中身が有効なコンポーネントだけ取り出す。Enabledもここでチェック。
                var enabledComponents = components.Where(x => x != null && x.Enabled && x.targetTexture != null);
                if (enabledComponents == null || !enabledComponents.Any()) return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(null));

                // このアバター配下の全てのRendererが使っている全てのテクスチャのハッシュ一覧
                var avatarTexturesHashSet = TextureUtils.GetAvatarTexturesHashSet(avatar);
                if (avatarTexturesHashSet == null || !avatarTexturesHashSet.Any()) return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(null));

                // 変更される予定のテクスチャ（アバター配下で使われている物だけ）
                var targetTextures = enabledComponents
                    .Select(c => c.targetTexture)
                    .Where(t => avatarTexturesHashSet.Contains(t))
                    .Distinct()
                    .ToArray();
                if (targetTextures == null || !targetTextures.Any()) return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(null));

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

        //このノードはアバター1体につき1個作られる。OnFrameは、RenderGroupの中身分のみ呼ばれる
        private class TextureReplacerNode : IRenderFilterNode, IDisposable
        {
            private readonly Dictionary<Material, Material> _processedMaterialsDictionary;

            public RenderAspects WhatChanged { get; private set; } = RenderAspects.Texture & RenderAspects.Material;

            public TextureReplacerNode(Dictionary<Material, Material> materialDictionary)
            {
                if (materialDictionary == null) return;
                _processedMaterialsDictionary = materialDictionary; //ここで渡されるものは、OnFrameで、置き換えられるものがあるのが確定したマテリアルと、その処理済みマテリアルのDictionaryである
            }

            public void OnFrame(Renderer original, Renderer proxy)
            {
                try
                {
                    if (proxy == null || proxy.sharedMaterials == null || _processedMaterialsDictionary == null || _processedMaterialsDictionary.Count == 0)
                        return;

                    proxy.sharedMaterials = GenerateSwappedMaterials(proxy.sharedMaterials);
                }
                catch (Exception ex)
                {
                    LogUtils.LogError("Error occurred while rendering proxy.\n" + ex);
                }
            }

            private Material[] GenerateSwappedMaterials(Material[] proxyMaterials)
            {
                var processedMaterials = new Material[proxyMaterials.Length];

                for (int i = 0; i < proxyMaterials.Length; i++)
                {
                    var proxyMaterial = proxyMaterials[i];
                    if (proxyMaterial == null)
                    {
                        processedMaterials[i] = null;
                        continue;
                    }

                    if (_processedMaterialsDictionary.TryGetValue(proxyMaterial, out var processedMaterial))
                    {
                        processedMaterials[i] = processedMaterial;
                    }
                    else
                    {
                        processedMaterials[i] = proxyMaterial;
                    }
                }

                return processedMaterials;
            }

            public void Dispose()
            {
                foreach (var material in _processedMaterialsDictionary.Values)
                {
                    MaterialUtils.ForEachTex(material, (texture, propName) =>
                    {
                        if (texture is not RenderTexture rt) return;

                        rt.DiscardContents();
                        Object.DestroyImmediate(rt);
                    });

                    Object.DestroyImmediate(material);
                }

                _processedMaterialsDictionary.Clear();
            }
        }
    }
}
