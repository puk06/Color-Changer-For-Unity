using System;
using System.Collections.Generic;
using System.Linq;
using net.puk06.ColorChanger.Editor.Localization;
using UnityEditor;

namespace net.puk06.ColorChanger.Editor.Utils
{
    internal static class LocalizationUtils
    {
        internal static void DrawLanguageSelectionPopup()
        {
            List<(string, string)> languages = Localizer.Instance.Languages;

            int currentIndex = Array.IndexOf(languages.Select(lang => lang.Item1).ToArray(), LocalizationManager.CurrentLanguage);
            int newIndex = EditorGUILayout.Popup("Language", currentIndex, languages.Select(lang => lang.Item2).ToArray());

            if (newIndex != currentIndex && newIndex >= 0)
            {
                LocalizationManager.CurrentLanguage = languages[newIndex].Item1;
            }
        }

        internal static string Localize(string key)
        {
            try
            {
                return Localizer.Instance.Get(LocalizationManager.CurrentLanguage, key);
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
