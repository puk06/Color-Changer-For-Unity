using nadena.dev.ndmf.preview;
using net.puk06.ColorChanger.ImageProcessing;
using net.puk06.ColorChanger.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace net.puk06.ColorChanger.NDMF {
    internal class NDMFPreview : IRenderFilter
    {
        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            var avatars = context.GetAvatarRoots();
            var resultSet = new HashSet<RenderGroup>();

            foreach (var avatar in avatars)
            {
                var renderers = avatar.GetComponentsInChildren<Renderer>();
                var colorChangers = avatar.GetComponentsInChildren<ColorChangerForUnity>();

                foreach (var component in colorChangers)
                {
                    if (component.targetTexture == null) continue;

                    string targetPath = AssetDatabase.GetAssetPath(component.targetTexture);
                    if (string.IsNullOrEmpty(targetPath)) continue;

                    foreach (var renderer in renderers)
                    {
                        if (RendererHasTexture(renderer, targetPath))
                        {
                            var group = RenderGroup.For(renderer).WithData(component);
                            if (!resultSet.Contains(group))
                            {
                                resultSet.Add(group);
                            }
                        }
                    }
                }
            }

            return resultSet.ToImmutableList();
        }


        private bool RendererHasTexture(Renderer renderer, string targetPath)
        {
            var materials = renderer.sharedMaterials;
            foreach (var material in materials)
            {
                if (material == null) continue;
                var shader = material.shader;
                int count = ShaderUtil.GetPropertyCount(shader);

                for (int i = 0; i < count; i++)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) != ShaderUtil.ShaderPropertyType.TexEnv)
                        continue;

                    string propName = ShaderUtil.GetPropertyName(shader, i);
                    Texture currentTex = material.GetTexture(propName);
                    if (currentTex == null) continue;

                    string currentPath = AssetDatabase.GetAssetPath(currentTex);
                    if (currentPath == targetPath)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            Dictionary<Material, Material> materialDict = new();
            try
            {
                var component = group.GetData<ColorChangerForUnity>();

                var originalTexture = component.targetTexture;
                var originalTexturePath = AssetDatabase.GetAssetPath(originalTexture);

                var processedTexture = ComputeTextureOverrides(component);
                if (processedTexture == null) return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(materialDict));

                foreach (var proxyPair in proxyPairs)
                {
                    var originalMaterials = proxyPair.Item2.sharedMaterials;

                    foreach (var originalMaterial in originalMaterials)
                    {
                        if (materialDict.ContainsKey(originalMaterial)) continue;

                        var newMaterial = new Material(originalMaterial);
                        materialDict.Add(originalMaterial, newMaterial);

                        Shader shader = newMaterial.shader;
                        int count = ShaderUtil.GetPropertyCount(shader);

                        for (int i = 0; i < count; i++)
                        {
                            if (ShaderUtil.GetPropertyType(shader, i) != ShaderUtil.ShaderPropertyType.TexEnv)
                                continue;

                            string propName = ShaderUtil.GetPropertyName(shader, i);
                            Texture currentTex = newMaterial.GetTexture(propName);
                            string texturePath = AssetDatabase.GetAssetPath(currentTex);

                            if (texturePath != originalTexturePath) continue;
                            newMaterial.SetTexture(propName, processedTexture);
                        }
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
            RenderTexture rawTexture = ConvertToNonCompressed(component.targetTexture);

            RenderTexture newTex = new RenderTexture(component.targetTexture.width, component.targetTexture.height, 0);
            newTex.enableRandomWrite = true;
            newTex.Create();

            var colorDiff = new ColorDifference(component.previousColor, component.newColor);
            var processor = new ImageProcessor(colorDiff);

            if (component.balanceModeConfiguration.ModeVersion != 0)
                processor.SetBalanceSettings(component.balanceModeConfiguration);

            if (component.advancedColorConfiguration.Enabled)
                processor.SetColorSettings(component.advancedColorConfiguration);

            processor.ProcessAllPixelsGPU(rawTexture, newTex);

            return newTex;
        }

        private static RenderTexture ConvertToNonCompressed(Texture2D source)
        {
            var rt = new RenderTexture(source.width, source.height, 0);
            rt.enableRandomWrite = true;
            rt.Create();

            Graphics.Blit(source, rt);
            return rt;
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

                    for (int i = 0; i < proxy.sharedMaterials.Length; i++)
                    {
                        var material = proxy.sharedMaterials[i];
                        if (!_materialDictionary.ContainsKey(material))
                            return;

                        proxy.sharedMaterials[i] = _materialDictionary[material];
                    }
                } catch { }
            }

            public void Dispose()
            {
                foreach (var material in _materialDictionary.Values)
                {
                    Object.DestroyImmediate(material);
                }

                _materialDictionary.Clear();
            }
        }
    }
}
