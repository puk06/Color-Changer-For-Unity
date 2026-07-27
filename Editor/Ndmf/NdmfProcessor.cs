#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using nadena.dev.ndmf;
using net.puk06.ColorChanger.Editor.Extension;
using net.puk06.ColorChanger.Editor.Models;
using net.puk06.ColorChanger.Editor.Services;
using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Ndmf
{
    internal class NdmfProcessor
    {
        internal static Dictionary<Texture2D, ExtendedRenderTexture> ProcessAllComponents(IEnumerable<ColorChangerForUnity> components, Action<ColorChangerForUnity>? onSuccess = null, Action<ColorChangerForUnity>? onFailed = null, bool isPreview = false)
        {
            var result = new Dictionary<Texture2D, ExtendedRenderTexture>();

            foreach (var component in components)
            {
                if (!component.IsActiveCCComponent(isPreview: isPreview)) continue;

                if (component.TargetTexture != null)
                {
                    if (result.ContainsKey(component.TargetTexture))
                    {
                        onFailed?.Invoke(component);
                    }
                    else if (component.ComponentTexture != null)
                    {
                        var processedTexture = TextureBuilder.Build(component.ComponentTexture, component, component.MaskTexture != null);
                        if (processedTexture != null)
                        {
                            result.Add(component.TargetTexture, processedTexture);
                            onSuccess?.Invoke(component);
                        }
                        else
                        {
                            onFailed?.Invoke(component);
                        }
                    }
                }

                foreach (var settingsInheritedTexture in component.SettingsInheritedTextures)
                {
                    if (settingsInheritedTexture == null) continue;
                    
                    if (result.ContainsKey(settingsInheritedTexture))
                    {
                        onFailed?.Invoke(component);
                    }
                    else
                    {
                        var processedTexture = TextureBuilder.Build(settingsInheritedTexture, component, false);
                        if (processedTexture != null)
                        {
                            result.Add(settingsInheritedTexture, processedTexture);
                            onSuccess?.Invoke(component);
                        }
                        else
                        {
                            onFailed?.Invoke(component);
                        }
                    }
                }
            }

            return result;
        }

        internal static Dictionary<Texture2D, Texture2D> ConvertToTexture2DDictionary(Dictionary<Texture2D, ExtendedRenderTexture> processedTexturesDictionary)
        {
            var result = new Dictionary<Texture2D, Texture2D>();

            foreach (var processedKpv in processedTexturesDictionary)
            {
                var convertedTexture = processedKpv.Value.ToTexture2D();
                processedKpv.Value.Dispose();

                result.Add(processedKpv.Key, convertedTexture);
            }

            return result;
        }

        internal static void ReplaceTexturesInRenderers(IEnumerable<Renderer> renderers, Dictionary<Texture2D, Texture2D> processedTexturesDictionary)
        {
            if (processedTexturesDictionary.Count == 0) return;

            var materialMap = new Dictionary<Material, Material>();
            
            foreach (var renderer in renderers)
            {
                Material?[] materials = renderer.sharedMaterials;
                bool changed = false;

                foreach (ref var material in materials.AsSpan())
                {
                    if (material == null) continue;
                    if (materialMap.TryGetValue(material, out Material? cloned))
                    {
                        material = cloned;
                        changed = true;
                    }
                    else
                    {
                        var newMaterial = GetProcessedMaterial(material, processedTexturesDictionary);
                        if (newMaterial == material) continue;

                        ObjectRegistry.RegisterReplacedObject(material, newMaterial!);
                        materialMap.Add(material, newMaterial!);
                        material = newMaterial;
                        changed = true;
                    }
                }

                if (changed) renderer.sharedMaterials = materials;
            }
        }

        [return:NotNullIfNotNull("material")]
        internal static Material? GetProcessedMaterial<T>(Material? material, Dictionary<Texture2D, T> processedTextures)
            where T : Texture
        {
            if (material == null) return null;

            Material? newMaterial = null;

            material.ForEachTexture((texture, propName) =>
            {
                if (texture is not Texture2D originalTexture || !processedTextures.TryGetValue(originalTexture, out T processedTexture)) return;
                if (newMaterial == null) newMaterial = UnityEngine.Object.Instantiate(material);
                newMaterial.SetTexture(propName, processedTexture);
            });

            if (newMaterial != null) return newMaterial;
            return material;
        }
    }
}
