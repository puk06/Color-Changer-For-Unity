using nadena.dev.ndmf;
using net.puk06.ColorChanger.ImageProcessing;
using net.puk06.ColorChanger.Models;
using UnityEditor;
using UnityEngine;

namespace net.puk06.ColorChanger.NDMF
{
    public class GenerateColorChangedTexture : Pass<GenerateColorChangedTexture>
    {
        protected override void Execute(BuildContext buildContext)
        {
            var avatar = buildContext.AvatarRootObject;

            foreach (var component in avatar.GetComponentsInChildren<ColorChangerForUnity>())
            {
                Texture2D originalTexture = ConvertToNonCompressed(component.targetTexture);
                Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);

                ColorDifference colorDifference = new ColorDifference(component.previousColor, component.newColor);
                ImageProcessor imageProcessor = new ImageProcessor(colorDifference);

                if (component.balanceModeConfiguration.ModeVersion != 0)
                    imageProcessor.SetBalanceSettings(component.balanceModeConfiguration);

                if (component.advancedColorConfiguration.Enabled)
                    imageProcessor.SetColorSettings(component.advancedColorConfiguration);

                imageProcessor.ProcessAllPixels(originalTexture, newTexture);

                AssetDatabase.AddObjectToAsset(newTexture, buildContext.AssetContainer);

                Renderer[] renderers = avatar.GetComponentsInChildren<Renderer>();

                foreach (var renderer in renderers)
                {
                    Material[] materials = renderer.materials;

                    foreach (var material in materials)
                    {
                        Shader shader = material.shader;
                        int count = ShaderUtil.GetPropertyCount(shader);

                        for (int i = 0; i < count; i++)
                        {
                            if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                            {
                                string propName = ShaderUtil.GetPropertyName(shader, i);
                                Texture currentTex = material.GetTexture(propName);

                                string path1 = AssetDatabase.GetAssetPath(currentTex);
                                string path2 = AssetDatabase.GetAssetPath(component.targetTexture);

                                if (path1 == path2)
                                {
                                    material.SetTexture(propName, newTexture);
                                }
                            }
                        }
                    }
                }

                Object.DestroyImmediate(component);
            }
        }
        
        private Texture2D ConvertToNonCompressed(Texture2D source)
        {
            RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default);
            Graphics.Blit(source, rt);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D readableTexture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);

            readableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readableTexture.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return readableTexture;
        }
    }
}
