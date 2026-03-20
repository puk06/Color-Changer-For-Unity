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
        internal static Dictionary<Texture2D, ExtendedRenderTexture> ProcessAllComponents(IEnumerable<ColorChangerForUnity> components, Action<ColorChangerForUnity>? onSuccess = null, Action<ColorChangerForUnity>? onFailed = null)
        {
            Dictionary<Texture2D, ExtendedRenderTexture> result = new();

            foreach (ColorChangerForUnity component in components)
            {
                if (component.TargetTexture != null)
                {
                    if (result.ContainsKey(component.TargetTexture))
                    {
                        onFailed?.Invoke(component);
                    }
                    else if (component.ComponentTexture != null)
                    {
                        ExtendedRenderTexture? processedTexture = TextureBuilder.Build(component.ComponentTexture, component, component.MaskTexture != null);
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

                foreach (Texture2D? settingsInheritedTexture in component.SettingsInheritedTextures)
                {
                    if (settingsInheritedTexture == null) continue;
                    
                    if (result.ContainsKey(settingsInheritedTexture))
                    {
                        onFailed?.Invoke(component);
                    }
                    else
                    {
                        ExtendedRenderTexture? processedTexture = TextureBuilder.Build(settingsInheritedTexture, component, false);
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
            Dictionary<Texture2D, Texture2D> result = new();

            foreach (KeyValuePair<Texture2D, ExtendedRenderTexture> processedKpv in processedTexturesDictionary)
            {
                Texture2D convertedTexture = processedKpv.Value.ToTexture2D();
                processedKpv.Value.Dispose();

                result.Add(processedKpv.Key, convertedTexture);
            }

            return result;
        }

        internal static void ReplaceTexturesInRenderers(IEnumerable<Renderer> renderers, Dictionary<Texture2D, Texture2D> processedTexturesDictionary)
        {
            Dictionary<Material, Material> materialMap = new();
            
            foreach (Renderer renderer in renderers)
            {
                Material?[] materials = renderer.sharedMaterials;

                foreach (ref Material? material in materials.AsSpan())
                {
                    if (material == null) continue;
                    if (materialMap.TryGetValue(material, out Material? cloned))
                    {
                        material = cloned;
                    }
                    else
                    {
                        Material newMaterial = GetProcessedMaterial(material, processedTexturesDictionary);

                        ObjectRegistry.RegisterReplacedObject(material, newMaterial);
                        materialMap.Add(material, newMaterial);
                        material = newMaterial;
                    }
                }

                renderer.sharedMaterials = materials;
            }
        }

        [return:NotNullIfNotNull("material")]
        internal static Material? GetProcessedMaterial<T>(Material? material, Dictionary<Texture2D, T> processedTextures)
            where T : Texture
        {
            if (material == null) return null;

            Material newMaterial = UnityEngine.Object.Instantiate(material);

            newMaterial.ForEachTexture((texture, propName) =>
            {
                if (texture is not Texture2D originalTexture || !processedTextures.TryGetValue(originalTexture, out T processedTexture)) return;
                newMaterial.SetTexture(propName, processedTexture);
            });

            return newMaterial;
        }
    }
}
