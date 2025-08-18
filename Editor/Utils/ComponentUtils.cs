namespace net.puk06.ColorChanger.Utils
{
    internal static class ColorChangerUtils
    {
        /// <summary>
        /// コンポーネントが有効状態にあるかを返します。PreviewEnabledはチェックされないので注意。
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        internal static bool IsEnabled(ColorChangerForUnity component)
            => component != null && component.Enabled && component.targetTexture != null && component.gameObject != null && component.gameObject.activeSelf;
    }
}
