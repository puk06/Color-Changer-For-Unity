using System;
using net.puk06.ColorChanger.Localization;
using System.Linq;
using UnityEditor;

namespace net.puk06.ColorChanger.Utils
{
    internal static class LocalizationUtils
    {
        internal static void GenerateLanguagePopup()
        {
            var langs = LocalizationManager.GetSupportedLanguages();
            int currentIndex = Array.IndexOf(langs.Select(lang => lang.Item2).ToArray(), LocalizationManager.CurrentLanguage);

            int newIndex = EditorGUILayout.Popup("Language", currentIndex, langs.Select(lang => lang.Item1).ToArray());

            if (newIndex != currentIndex && newIndex >= 0)
            {
                LocalizationManager.CurrentLanguage = langs[newIndex].Item2;
            }
        }
    }
}
