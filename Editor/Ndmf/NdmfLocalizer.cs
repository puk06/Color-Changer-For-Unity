using System;
using System.Collections.Generic;
using nadena.dev.ndmf.localization;

namespace net.puk06.ColorChanger.Editor.Ndmf
{
    internal static class NdmfLocalizer
    {
        internal static readonly Localizer Localizer = new("en", () =>
        {
            return new List<(string, Func<string, string>)>
            {
                ("en", key => Localization.Localizer.Instance.Get("en-US", key)),
                ("ja", key => Localization.Localizer.Instance.Get("ja-JP", key))
            };
        });
    }
}
