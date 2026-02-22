using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Utils
{
    internal static class DummyRenderTexture
    {
        private static RenderTexture _instance;
        internal static RenderTexture Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RenderTexture(1, 1, 0);
                    _instance.enableRandomWrite = true;
                    _instance.Create();
                }

                return _instance;
            }
        }
    }
}
