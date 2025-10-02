using System;
using System.Collections.Generic;
using nadena.dev.ndmf.localization;

namespace net.puk06.ColorChanger.Localization
{
    public class ColorChangerLocalizer
    {
        private static readonly Localizer _localizer;

        static ColorChangerLocalizer()
        {
            _localizer = new Localizer("en", () =>
            {
                var enDict = new Dictionary<string, string>
                {
                    { "colorchanger.process.success", "Texture Processing done\nComponent: {0}\nTexture: {1}\nProcessing time: {2} ms" },
                    { "colorchanger.process.error", "Texture Processing Error. See the console for details.\nComponent: {0}\nTexture: {1}\nProcessing time: {2} ms" }
                };

                var jaDict = new Dictionary<string, string>
                {
                    { "colorchanger.process.success", "テクスチャ生成が完了しました\nコンポーネント: {0}\nテクスチャ: {1}\n処理時間: {2} ms" },
                    { "colorchanger.process.error", "テクスチャ生成中にエラーが発生しました。詳細はコンソールをご覧ください\nコンポーネント: {0}\nテクスチャ: {1}\n処理時間: {2} ms" }
                };

                return new List<(string, Func<string, string>)>
                {
                    ("en", key => enDict.TryGetValue(key, out var val) ? val : null),
                    ("ja", key => jaDict.TryGetValue(key, out var val) ? val : null)
                };
            });
        }

        public static Localizer GetLocalizer() => _localizer;
    }
}
