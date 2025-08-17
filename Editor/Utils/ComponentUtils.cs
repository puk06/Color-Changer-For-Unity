namespace net.puk06.ColorChanger.Utils
{
    internal static class ColorChangerUtils
    {
        internal static bool IsEnabled(ColorChangerForUnity component)
            => component != null && component.Enabled && component.targetTexture != null && component.gameObject != null && component.gameObject.activeSelf;
    }
}
