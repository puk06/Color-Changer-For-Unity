using System.Linq;
using net.puk06.ColorChanger.Utils;
using UnityEditor;

namespace net.puk06.ColorChanger.Localization
{
    public static class LocalizationManager
    {
        private const string LanguageKey = "ColorChanger_CurrentLanguage";

        private static readonly (string, string)[] SupportedLanguages = { ("日本語", "ja"), ("English", "en"), ("한글", "ko") };

        public static string CurrentLanguage
        {
            get => EditorPrefs.GetString(LanguageKey, "ja");
            set
            {
                if (SupportedLanguages.Any(item => item.Item2.Contains(value)))
                    EditorPrefs.SetString(LanguageKey, value);
            }
        }

        public static (string, string)[] GetSupportedLanguages() => SupportedLanguages;

        public static string Get(string key, params string[] parameters)
        {
            if (!ToolLocalizer.LocalizationDictionary.TryGetValue(key, out var localizationData))
            {
                LogUtils.LogWarning($"Unknown Localization Key Found: {key}");
                return "Unknown";
            }

            var currentLanguage = CurrentLanguage;
            if (!localizationData.TryGetValue(currentLanguage, out var translatedString)) return "Unknown Language";

            return ReplaceParameters(translatedString, parameters);
        }

        private static string ReplaceParameters(string rawValue, string[] parameters)
        {
            if (parameters.Length != 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    var replaceString = $"{{{i}}}";
                    rawValue = rawValue.Replace(replaceString, parameters[i]);
                }
            }

            return rawValue;
        }
    }
}
