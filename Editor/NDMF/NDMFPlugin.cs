#nullable enable
using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using net.puk06.ColorChanger.Editor.Models;
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
            GameObject avatar = context.AvatarRootObject;
            ColorChangerForUnity[] components = avatar.GetComponentsInChildren<ColorChangerForUnity>(true)
#if USE_TEXTRANSTOOL
                .Where(component => !component.GetComponent<rs64.TexTransTool.MultiLayerImage.ExternalToolAsLayer>())
                .ToArray()
#endif
                ;

            IEnumerable<ColorChangerForUnity> enabledComponents = components.Where(x => x.gameObject.activeSelf && x.Enabled);
            Dictionary<Texture2D, ExtendedRenderTexture> processedTexturesDictionary = CCProcessor.ProcessAllTextures(enabledComponents,
                onSuccess: component =>
                {
                    string textureName = component.TargetTexture == null ? "Unknown Texture" : component.TargetTexture.name;
                    ErrorReport.ReportError(NdmfLocalizer.Localizer, ErrorSeverity.Information, "NdmfBuild.Processing.Success", component, textureName);
                },
                onFailed: component =>
                {
                    string textureName = component.TargetTexture == null ? "Unknown Texture" : component.TargetTexture.name;
                    ErrorReport.ReportError(NdmfLocalizer.Localizer, ErrorSeverity.NonFatal, "NdmfBuild.Processing.Failed", component, textureName);
                }
            );
            IEnumerable<Renderer> renderers = avatar.GetComponentsInChildren<Renderer>().Where(r => r is MeshRenderer or SkinnedMeshRenderer);
            CCProcessor.ReplaceTexturesInRenderers(renderers, CCProcessor.ConvertToTexture2DDictionary(processedTexturesDictionary));
        }
    }

    public class RemoveComponents : Pass<RemoveComponents>
    {
        protected override void Execute(BuildContext buildContext)
        {
            GameObject avatar = buildContext.AvatarRootObject;

            ColorChangerForUnity[] components = avatar.GetComponentsInChildren<ColorChangerForUnity>(true);
            DeleteAllComponents(components);
        }

        private void DeleteAllComponents(ColorChangerForUnity[] components)
        {
            foreach (ColorChangerForUnity component in components)
            {
                if (component == null) continue;
                Object.DestroyImmediate(component);
            }
        }
    }
}
