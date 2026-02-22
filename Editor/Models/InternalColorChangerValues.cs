#nullable enable
using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Models
{
    internal class InternalColorChangerValues
    {
        internal ColorChangerForUnity ParentComponent = null!;
        internal Texture2D? SourceTexture = null;
        internal Texture2D? TargetTexture = null;
        internal bool UseMask = false;

        internal InternalColorChangerValues(ColorChangerForUnity component, Texture2D? sourceTexture, Texture2D? targetTexture, bool useMask)
        {
            ParentComponent = component;
            SourceTexture = sourceTexture;
            TargetTexture = targetTexture;
            UseMask = useMask;
        }
    }
}
