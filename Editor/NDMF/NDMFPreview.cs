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
                    var groupedRenderers = avatar.GetComponentsInChildren<Renderer>()
                        .Where(r => r is MeshRenderer or SkinnedMeshRenderer)
                        .GroupBy(r => r.gameObject);

                    // 1つのオブジェクトの中にRendererが複数入っていたときのエラー対策。いらないかもしれないけど、ループになるので念の為
                    // ここでSelectを使ってない理由は、groupedRender.First()を無駄にしたくなかったからです。
                    var renderer = new List<Renderer>();
                    foreach (var groupedRender in groupedRenderers)
                    {
                        var firstComponent = groupedRender.First();
                        if (groupedRender.Count() >= 2)
                        {
                            LogUtils.LogWarning($"Duplicate Renderer Gameobject detected: '{groupedRender.Key.name}' (using settings from '{firstComponent.GetType()}' component)");
                        }

                        renderer.Add(firstComponent);
                    }

                    resultSet.Add(RenderGroup.For(renderer));
                }
                catch (Exception ex)
                {
                    LogUtils.LogError($"Failed to add renderer for avatar '{avatar.name}'.\n{ex.Message}");
                }
            }

            return resultSet.ToImmutableList();
        }

        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            try
            {
                // // アバター内の全てのColorChangerForUnityコンポーネントを出してくる。
                // // 現状、コンポーネントが入ったオブジェクトを移動した後にこれが実行されてほしいのだが、実行されないという問題がある。
                // // ビルド時はアバタールートに入っている必要があるため、プレビューできないという問題が出てきてしまう。
                // var avatar = context.GetAvatarRoot(group.Renderers.First().gameObject);
                // if (avatar == null) return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(null));

                // var components = avatar.GetComponentsInChildren<ColorChangerForUnity>();
                // if (components.Length == 0) return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(null));

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
                    var firstComponent = groupedComponent.First();
                    if (groupedComponent.Count() >= 2)
                    {
                        LogUtils.LogWarning($"Duplicate targetTexture detected: '{groupedComponent.Key.name}' (using settings from '{firstComponent.gameObject.name}')");
                    }

                    // テクスチャを作る
                    var processedTexture = ComputeTextureOverrides(firstComponent);
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

                // foreach (var component in components)
                // {
                //     context.Observe(component);
                // }

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
                    LogUtils.LogError("Error occurred while rendering proxy.\n" + ex.Message);
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
