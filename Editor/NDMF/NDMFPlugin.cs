using nadena.dev.ndmf;
using net.puk06.ColorChanger.NDMF;

[assembly: ExportsPlugin(typeof(NDMFPlugin))]

namespace net.puk06.ColorChanger.NDMF
{
    internal class NDMFPlugin : Plugin<NDMFPlugin>
    {
        public override string QualifiedName => "net.puk06.color-changer";
        public override string DisplayName => "Color Changer For Unity";

        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                .Run(GenerateColorChangedTexture.Instance)
                .PreviewingWith(new NDMFPreview());
        }
    }
}
