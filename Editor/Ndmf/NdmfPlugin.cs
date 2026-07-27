#nullable enable
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.util;
using net.puk06.ColorChanger.Editor.Ndmf;
using UnityEngine;

[assembly: ExportsPlugin(typeof(NdmfPlugin))]
namespace net.puk06.ColorChanger.Editor.Ndmf
{
    internal class NdmfPlugin : Plugin<NdmfPlugin>
    {
        public override string QualifiedName => "net.puk06.color-changer";
        public override string DisplayName => "Color Changer For Unity";

        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                .AfterPlugin("net.rs64.tex-trans-tool")
                .AfterPlugin("nadena.dev.modular-avatar")
                .Run(BuildTextures.Instance)
#if LLC_2_4_0_OR_NEWER
                .BeforePass("io.github.azukimochi.light-limit-changer.normalize-materials")
#endif
                .PreviewingWith(new RealtimePreview());

            InPhase(BuildPhase.Optimizing)
                .AfterPlugin("net.rs64.tex-trans-tool")
                .BeforePlugin("com.anatawa12.avatar-optimizer")
                .Run(RemoveComponents.Instance);
        }
    }

    internal class BuildTextures : Pass<BuildTextures>
    {
        protected override void Execute(BuildContext context)
        {
            var avatar = context.AvatarRootObject;
            var components = avatar.GetComponentsInChildren<ColorChangerForUnity>(false)
#if USE_TEXTRANSTOOL
                .Where(component => !component.GetComponent<rs64.TexTransTool.MultiLayerImage.ExternalToolAsLayer>())
                .ToArray()
#endif
                ;

            var processedTexturesDictionary = NdmfProcessor.ProcessAllComponents(components,
                onSuccess: component =>
                {
                    string textureName = component.TargetTexture == null ? "Unknown Texture" : component.TargetTexture.name;
                    ErrorReport.ReportError(NdmfLocalizer.Localizer, ErrorSeverity.Information, "NdmfBuild.Processing.Success", component.AvatarRootPath(), textureName);
                },
                onFailed: component =>
                {
                    string textureName = component.TargetTexture == null ? "Unknown Texture" : component.TargetTexture.name;
                    ErrorReport.ReportError(NdmfLocalizer.Localizer, ErrorSeverity.NonFatal, "NdmfBuild.Processing.Failed", component.AvatarRootPath(), textureName);
                }
            );
            var renderers = avatar.GetComponentsInChildren<Renderer>(true).Where(r => r is MeshRenderer or SkinnedMeshRenderer);
            var texture2DDictionary = NdmfProcessor.ConvertToTexture2DDictionary(processedTexturesDictionary);
            NdmfProcessor.ReplaceTexturesInRenderers(renderers, texture2DDictionary);

            foreach (var texture in texture2DDictionary.Values)
                context.AssetSaver.SaveAsset(texture);
        }
    }

    public class RemoveComponents : Pass<RemoveComponents>
    {
        protected override void Execute(BuildContext buildContext)
        {
            var avatar = buildContext.AvatarRootObject;

            var components = avatar.GetComponentsInChildren<ColorChangerForUnity>(true);
            DeleteAllComponents(components);
        }

        private void DeleteAllComponents(ColorChangerForUnity[] components)
        {
            foreach (var component in components)
            {
                if (component == null) continue;
                Object.DestroyImmediate(component);
            }
        }
    }
}
