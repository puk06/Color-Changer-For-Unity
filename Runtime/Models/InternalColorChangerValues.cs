#nullable enable
using UnityEngine;

namespace net.puk06.ColorChanger.Models
{
    public class InternalColorChangerValues
    {
        public ColorChangerForUnity parentComponent = null!;
        public Texture2D? originalTexture = null;
        public Texture2D? targetTexture = null;
        public bool useMask = false;

        public InternalColorChangerValues(ColorChangerForUnity colorChangerForUnity, Texture2D? originalTexture, Texture2D? componentTexture, bool useMask)
        {
            parentComponent = colorChangerForUnity;
            this.originalTexture = originalTexture;
            targetTexture = componentTexture;
            this.useMask = useMask;
        }
    }
}
