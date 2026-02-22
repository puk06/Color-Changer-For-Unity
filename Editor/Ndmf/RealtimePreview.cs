#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using nadena.dev.ndmf.preview;
using net.puk06.ColorChanger.Editor.Extentions;
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

                    List<Texture2D> targetTextures = components
                        .Select(c => context.Observe(c, c => c.TargetTexture))
                        .Where(t => t != null)
                        .Distinct()
                        .ToList()!;

                    foreach (ColorChangerForUnity component in components)
                    {
                        foreach (Texture2D? otherTexture in component.SettingsInheritedTextures.Where(t => t != null).Distinct())
                        {
                            targetTextures.Add(context.Observe(otherTexture!));
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
            try
            {
                ColorChangerForUnity[] components = group.GetData<ColorChangerForUnity[]>();
                foreach (ColorChangerForUnity component in components) context.Observe(component);

                IEnumerable<ColorChangerForUnity> enabledComponents = components.Where(x => context.ActiveInHierarchy(x.gameObject) && x.Enabled && x.PreviewEnabled);
                Dictionary<Texture2D, ExtendedRenderTexture> processedTextures = CCProcessor.ProcessAllComponents(enabledComponents);
                ObjectReferenceService.RegisterReplacements(processedTextures);

                Dictionary<Renderer, Material?[]> processedMaterialDictionary = new();

                foreach ((Renderer original, Renderer proxy) in proxyPairs)
                {
                    processedMaterialDictionary[original] = proxy.sharedMaterials.Select(mat => CCProcessor.GetProcessedMaterial(mat, processedTextures)).ToArray();
                }

                return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(processedMaterialDictionary, processedTextures.Values));
            }
            catch (Exception ex)
            {
                LogUtils.LogError($"Failed to instantiate.\n{ex}");
                return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(null, null));
            }
        }

        private class TextureReplacerNode : IRenderFilterNode, IDisposable
        {
            private readonly IEnumerable<ExtendedRenderTexture>? _processedTextures;
            private readonly Dictionary<Renderer, Material?[]>? _processedMaterialDictionary;

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
                }

                if (_processedMaterialDictionary != null)
                {
                    foreach (Material?[] materials in _processedMaterialDictionary.Values)
                        foreach (Material? material in materials)
                            if (material != null) Object.DestroyImmediate(material);
                }
            }
        }
    }
}
