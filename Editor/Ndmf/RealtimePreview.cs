#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using nadena.dev.ndmf.preview;
using net.puk06.ColorChanger.Editor.Extension;
using net.puk06.ColorChanger.Editor.Models;
using net.puk06.ColorChanger.Editor.Utils;
using net.puk06.ColorChanger.Services;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace net.puk06.ColorChanger.Editor.Ndmf
{
    internal class RealtimePreview : IRenderFilter
    {
        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            ImmutableList<GameObject> avatars = context.GetAvatarRoots();

            List<RenderGroup> targetRenderGroups = new List<RenderGroup>();

            foreach (GameObject avatar in avatars)
            {
                try
                {
                    ColorChangerForUnity[] components = context.GetComponentsInChildren<ColorChangerForUnity>(avatar, true)
#if USE_TEXTRANSTOOL
                        .Where(component => !context.GetComponent<rs64.TexTransTool.MultiLayerImage.ExternalToolAsLayer>(component.gameObject))
                        .ToArray()
#endif
                    ;
                    if (components.Length == 0) continue;

                    List<Texture2D> targetTextures = new();

                    foreach (ColorChangerForUnity component in components)
                    {
                        if (component.TargetTexture != null)
                        {
                            if (component.TargetTexture != null && !targetTextures.Contains(component.TargetTexture))
                            {
                                targetTextures.Add(context.Observe(component.TargetTexture));
                            }
                        }

                        foreach (Texture2D? settingsInheritedTexture in component.SettingsInheritedTextures)
                        {
                            if (settingsInheritedTexture == null || targetTextures.Contains(settingsInheritedTexture)) continue;
                            targetTextures.Add(context.Observe(settingsInheritedTexture!));
                        }
                    }

                    List<Renderer> targetRenderers = new();
                    foreach (Renderer avatarRenderer in avatar.GetComponentsInChildren<Renderer>().Where(r => r is MeshRenderer or SkinnedMeshRenderer))
                    {
                        Material[] materials = avatarRenderer.sharedMaterials;
                        if (materials == null) continue;

                        if (materials.Any(material => targetTextures.Any(targetTexture => targetTexture != null && material.HasTexture(targetTexture))))
                        {
                            targetRenderers.Add(avatarRenderer);
                        }
                    }

                    if (targetRenderers.Count > 0)
                    {
                        targetRenderGroups.Add(RenderGroup.For(targetRenderers).WithData(components));
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
            Dictionary<Texture2D, ExtendedRenderTexture>? processedTexturesDictionary = null;
            Dictionary<Renderer, Material?[]>? processedMaterialDictionary = new();

            try
            {
                ColorChangerForUnity[] components = group.GetData<ColorChangerForUnity[]>();
                foreach (ColorChangerForUnity component in components) context.Observe(component);

                IEnumerable<ColorChangerForUnity> enabledComponents = components.Where(x => context.ActiveInHierarchy(x.gameObject) && x.IsEnabled && x.IsPreviewEnabled);
                Dictionary<Texture2D, ExtendedRenderTexture> processedTextures = NdmfProcessor.ProcessAllComponents(enabledComponents);
                ObjectReferenceService.RegisterReplacements(processedTextures);

                foreach ((Renderer original, Renderer proxy) in proxyPairs)
                {
                    processedMaterialDictionary[original] = proxy.sharedMaterials.Select(mat => NdmfProcessor.GetProcessedMaterial(mat, processedTextures)).ToArray();
                }

                return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(processedMaterialDictionary, processedTextures.Values));
            }
            catch (Exception ex)
            {
                LogUtils.LogError($"Failed to instantiate.\n{ex}");
                if (processedTexturesDictionary != null)
                {
                    foreach (ExtendedRenderTexture texture in processedTexturesDictionary.Values)
                        texture.Dispose();
                    processedTexturesDictionary.Clear();
                    processedTexturesDictionary = null;
                }

                if (processedMaterialDictionary != null)
                {
                    foreach (Material?[] materials in processedMaterialDictionary.Values)
                        foreach (Material? material in materials)
                            if (material != null) Object.DestroyImmediate(material);
                    processedMaterialDictionary.Clear();
                    processedMaterialDictionary = null;
                }
                return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(null, null));
            }
        }

        private class TextureReplacerNode : IRenderFilterNode, IDisposable
        {
            private IEnumerable<ExtendedRenderTexture>? _processedTextures;
            private Dictionary<Renderer, Material?[]>? _processedMaterialDictionary;

            public RenderAspects WhatChanged { get; private set; } = RenderAspects.Texture & RenderAspects.Material;

            public TextureReplacerNode(Dictionary<Renderer, Material?[]>? processedMaterialDictionary, IEnumerable<ExtendedRenderTexture>? processedTexturesDictionary)
            {
                _processedMaterialDictionary = processedMaterialDictionary;
                _processedTextures = processedTexturesDictionary;
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
                if (_processedTextures != null)
                {
                    foreach (ExtendedRenderTexture texture in _processedTextures)
                        texture.Dispose();
                    _processedTextures = null;
                }

                if (_processedMaterialDictionary != null)
                {
                    foreach (Material?[] materials in _processedMaterialDictionary.Values)
                        foreach (Material? material in materials)
                            if (material != null) Object.DestroyImmediate(material);
                    _processedMaterialDictionary.Clear();
                    _processedMaterialDictionary = null;
                }
            }
        }
    }
}
