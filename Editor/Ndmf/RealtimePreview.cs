#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using nadena.dev.ndmf.preview;
using net.puk06.ColorChanger.Editor.Extension;
using net.puk06.ColorChanger.Editor.Utils;
using net.puk06.ColorChanger.Services;
using UnityEngine;
using Object = UnityEngine.Object;

namespace net.puk06.ColorChanger.Editor.Ndmf
{
    internal class RealtimePreview : IRenderFilter
    {
        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            var avatars = context.GetAvatarRoots().Distinct();

            var targetRenderGroups = new List<RenderGroup>();

            foreach (var avatar in avatars)
            {
                try
                {
                    var components = context.GetComponentsInChildren<ColorChangerForUnity>(avatar, true)
#if USE_TEXTRANSTOOL
                        .Where(component => !context.GetComponent<rs64.TexTransTool.MultiLayerImage.ExternalToolAsLayer>(component.gameObject))
                        .ToArray()
#endif
                    ;
                    if (components.Length == 0) continue;

                    var targetTextures = new HashSet<Texture2D>();

                    foreach (var component in components)
                    {
                        context.Observe(component, c => c.TargetTexture, (a, b) => a == b);
                        context.Observe(component, c => new List<Texture2D?>(c.SettingsInheritedTextures), (a, b) => a.SequenceEqual(b));

                        if (component.TargetTexture != null)
                        {
                            if (component.TargetTexture != null && !targetTextures.Contains(component.TargetTexture))
                            {
                                targetTextures.Add(component.TargetTexture);
                            }
                        }

                        foreach (var settingsInheritedTexture in component.SettingsInheritedTextures)
                        {
                            if (settingsInheritedTexture == null || targetTextures.Contains(settingsInheritedTexture)) continue;
                            targetTextures.Add(settingsInheritedTexture);
                        }
                    }

                    var targetRenderers = new List<Renderer>();
                    foreach (Renderer avatarRenderer in context.GetComponentsInChildren<Renderer>(avatar, true).Where(r => r is MeshRenderer or SkinnedMeshRenderer))
                    {
                        var materials = context.Observe(avatarRenderer, i => i.sharedMaterials, (a, b) => a != null && b != null && a.SequenceEqual(b));
                        if (materials == null) continue;

                        if (materials.Any(material => targetTextures.Any(targetTexture => targetTexture != null && material.HasTexture(targetTexture))))
                        {
                            targetRenderers.Add(avatarRenderer);
                        }
                    }

                    if (targetRenderers.Count > 0)
                    {
                        targetRenderGroups.Add(RenderGroup.For(targetRenderers).WithData(avatar));
                    }
                }
                catch (Exception ex)
                {
                    LogUtils.LogError($"Failed to add renderer for avatar: '{avatar.name}'.\n{ex}");
                }
            }

            return targetRenderGroups.ToImmutableList();
        }

        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            Dictionary<Texture2D, Texture2D>? processedTexturesDictionary = null;
            Dictionary<Renderer, Material?[]>? processedMaterialDictionary = new();
            Dictionary<Material, Material>? materialMap = null;

            try
            {
                var root = group.GetData<GameObject>();

                var components = root.GetComponentsInChildren<ColorChangerForUnity>(true);
                if (components.Length == 0) return Task.FromResult<IRenderFilterNode>(new EmptyNode());
                foreach (var component in components)
                {
                    context.Observe(component);
                    context.ActiveInHierarchy(component.gameObject);
                    context.Observe(component.gameObject, go => go.tag);
                }

                var processedTextures = NdmfProcessor.ProcessAllComponents(components, isPreview: true);
                processedTexturesDictionary = NdmfProcessor.ConvertToTexture2DDictionary(processedTextures);
                ObjectReferenceService.RegisterReplacements(processedTexturesDictionary);

                materialMap = new();

                foreach ((Renderer original, Renderer proxy) in proxyPairs)
                {
                    Material?[] materials = proxy.sharedMaterials;
                    Material?[] newMaterials = (Material?[])materials.Clone();
                    bool changed = false;

                    for (int i = 0; i < materials.Length; i++)
                    {
                        var material = materials[i];
                        if (material == null) continue;

                        if (materialMap.TryGetValue(material, out var cached))
                        {
                            newMaterials[i] = cached;
                            changed = true;
                        }
                        else
                        {
                            var processed = NdmfProcessor.GetProcessedMaterial(material, processedTexturesDictionary);
                            if (processed != material)
                            {
                                materialMap.Add(material, processed!);
                                newMaterials[i] = processed;
                                changed = true;
                            }
                        }
                    }

                    if (changed)
                        processedMaterialDictionary[original] = newMaterials;
                }

                return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(processedMaterialDictionary, materialMap.Values));
            }
            catch (Exception ex)
            {
                LogUtils.LogError($"Failed to instantiate.\n{ex}");
                if (processedTexturesDictionary != null)
                {
                    foreach (var texture in processedTexturesDictionary.Values)
                        Object.DestroyImmediate(texture);
                    processedTexturesDictionary.Clear();
                    processedTexturesDictionary = null;
                }

                if (processedMaterialDictionary != null)
                {
                    if (materialMap != null)
                    {
                        foreach (var material in materialMap.Values)
                            Object.DestroyImmediate(material);
                    }
                    processedMaterialDictionary.Clear();
                    processedMaterialDictionary = null;
                }
                return Task.FromResult<IRenderFilterNode>(new EmptyNode());
            }
        }

        private class TextureReplacerNode : IRenderFilterNode, IDisposable
        {
            private Dictionary<Renderer, Material?[]>? _processedMaterialDictionary;
            private IEnumerable<Material>? _createdMaterials;

            public RenderAspects WhatChanged { get; private set; } = RenderAspects.Texture | RenderAspects.Material;

            public TextureReplacerNode(Dictionary<Renderer, Material?[]>? processedMaterialDictionary, IEnumerable<Material>? createdMaterials)
            {
                _processedMaterialDictionary = processedMaterialDictionary;
                _createdMaterials = createdMaterials;
            }

            public void OnFrame(Renderer original, Renderer proxy)
            {
                try
                {
                    if (_processedMaterialDictionary?.TryGetValue(original, out Material?[] processedMaterials) ?? false)
                    {
                        proxy.sharedMaterials = processedMaterials;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error occurred while rendering proxy.\n" + ex);
                }
            }

            public void Dispose()
            {
                if (_createdMaterials != null)
                {
                    foreach (var material in _createdMaterials)
                        Object.DestroyImmediate(material);
                    _createdMaterials = null;
                }

                if (_processedMaterialDictionary != null)
                {
                    _processedMaterialDictionary.Clear();
                    _processedMaterialDictionary = null;
                }
            }
        }

        public class EmptyNode : IRenderFilterNode
        {
            public RenderAspects WhatChanged { get; private set; } = 0;

            public void OnFrame(Renderer original, Renderer proxy)
            {
                // Do nothing
            }
        }
    }
}
