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
            LogUtils.Log("GetTargetGroups Called!");
            var avatars = context.GetAvatarRoots();
            var resultSet = new List<RenderGroup>();

            foreach (var avatar in avatars)
            {
                var renderers = avatar.GetComponentsInChildren<Renderer>();
                var colorChangers = avatar.GetComponentsInChildren<ColorChangerForUnity>();

                foreach (var component in colorChangers)
                {
                    if (component.targetTexture == null) continue;

                    var targetRenderers = renderers
                        .Where(e => RendererHasTargetTexture(e, component.targetTexture));

                    resultSet.Add(RenderGroup.For(targetRenderers).WithData(component));

                    foreach (var renderer in renderers)
                    {
                        context.Observe(renderer.gameObject);
                    }

                    /**
                     * 下のリターンについて:
                     * 現在、複数のRenderGroupに渡って同じRendererが追加されてしまってエラーを吐きます。
                     * これをテスト環境で一時的にパスするためのリターンです。修正が終わり次第、削除されます。
                     * 
                     * 再現方法:
                     * 同じオブジェクトに複数のマテリアルが含まれており、ColorChangerForUnityコンポーネントがその複数のマテリアルのテクスチャを参照することでエラーを吐く。
                     * 
                     * Instantiateに渡されるとき、ColorChangerForUnityコンポーネントの個数分実行されることを想定しているので、これを治すには、根本的な部分を治す必要がある。
                     * どうしたものか...
                     */

                    return resultSet.ToImmutableList();
                }
            }

            LogUtils.Log("RenderGroup Count: " + resultSet.Count);

            return resultSet.ToImmutableList();
        }

        private bool RendererHasTargetTexture(Renderer renderer, Texture2D targetTexture)
        {
            var materials = renderer.sharedMaterials;
            return materials.Any(material => MaterialUtils.AnyTex(material, targetTexture));
        }

        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            LogUtils.Log("Instantiate Called!");
            Dictionary<Material, Material> materialDict = new();

            try
            {
                var component = group.GetData<ColorChangerForUnity>();
                var targetTexture = component.targetTexture;

                var processedTexture = ComputeTextureOverrides(component);
                if (processedTexture == null) return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(materialDict));

                foreach (var proxyPair in proxyPairs)
                {
                    var proxyMaterials = proxyPair.Item2.sharedMaterials;
                    var matchingMaterials = TextureUtils.FindMaterialsWithTexture(proxyMaterials, targetTexture);

                    foreach (var matchingMaterial in matchingMaterials)
                    {
                        if (materialDict.ContainsKey(matchingMaterial)) continue;

                        var newMaterial = new Material(matchingMaterial);
                        materialDict.Add(matchingMaterial, newMaterial);

                        MaterialUtils.ForEachTex(newMaterial, (texture, propName) =>
                        {
                            if (texture != targetTexture) return;
                            newMaterial.SetTexture(propName, processedTexture);
                        });
                    }
                }

                context.Observe(component);

                return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(materialDict));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(materialDict));
            }
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
                    if (proxy == null || _materialDictionary == null || _materialDictionary.Count == 0) return;

                    var newMaterials = new Material[proxy.sharedMaterials.Length];
                    for (int i = 0; i < proxy.sharedMaterials.Length; i++)
                    {
                        var material = proxy.sharedMaterials[i];
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
                catch
                {
                    // Ignored
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
