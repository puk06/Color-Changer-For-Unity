#nullable enable
using nadena.dev.ndmf;
using Object = UnityEngine.Object;

namespace net.puk06.ColorChanger.NDMF
{
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
                Object.DestroyImmediate(component);
            }
        }
    }
}
