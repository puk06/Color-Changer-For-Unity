#nullable enable
using UnityEngine;

namespace net.puk06.ColorChanger.Models
{
    public class InternalColorChangerValues
    {
        public ColorChangerForUnity parentComponent = null!;
        public Texture2D? targetTexture = null;

        public InternalColorChangerValues(ColorChangerForUnity colorChangerForUnity, Texture2D? componentTexture)
        {
            parentComponent = colorChangerForUnity;
            targetTexture = componentTexture;
        }
    }
}
