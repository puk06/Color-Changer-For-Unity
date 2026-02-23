#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Localization
{
    internal class Localizer
    {
        private readonly Dictionary<string, Dictionary<string, string>> _map;

        private readonly List<(string, string)> _languageMetaData = new();

        internal static Localizer Instance { get; private set; } = new Localizer();

        private Localizer()
        {
            _map = new();
            LoadFromFolder("Packages/net.puk06.color-changer/Editor/Localization/locales");
        }

        private void LoadFromFolder(string path)
        {
            if (!Directory.Exists(path)) return;

            List<Dictionary<string, string>> rawMapData = new();

            foreach (string filePath in Directory.GetFiles(path).Where(i => i.EndsWith(".json")))
            {
                try
                {
                    Dictionary<string, string>? deserializeResult = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(filePath));

                    if (deserializeResult == null)
                    {
                        Debug.LogError($"Failed to load language: '{Path.GetFileName(filePath)}'.");
                    }
                    else if (!deserializeResult.ContainsKey("LanguageName") || !deserializeResult.ContainsKey("LocalizedLanguageName"))
                    {
                        Debug.LogError($"Failed to load language: '{Path.GetFileName(filePath)}'. Couldn't get language name.");
                    }
                    else if (_map.ContainsKey(deserializeResult["LanguageName"]))
                    {
                        Debug.LogError($"Failed to load language: '{Path.GetFileName(filePath)}'. Already added language name: '{deserializeResult["LanguageName"]}'");
                    }
                    else
                    {
                        rawMapData.Add(deserializeResult);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load language: '{Path.GetFileName(filePath)}'.\n{ex}");
                }
            }

            static int TryGetInt(string? value, int defaultValue = 0)
            {
                if (string.IsNullOrEmpty(value)) return defaultValue;
                return int.TryParse(value, out int v) ? v : defaultValue;
            }

            List<Dictionary<string, string>> sortedMaps = rawMapData.OrderBy(i => TryGetInt(i.TryGetValue("LanguagePriority", out string? value) ? value : string.Empty, int.MaxValue)).ToList();
            foreach (Dictionary<string, string> languageData in sortedMaps)
            {
                _map[languageData["LanguageName"]] = languageData;
                _languageMetaData.Add((languageData["LanguageName"], languageData["LocalizedLanguageName"]));
            }
        }

        internal List<(string, string)> Languages => _languageMetaData;

        internal string? Get(string language, string localizationKey)
        {
            try
            {
                return _map[language][localizationKey];
            }
            catch
            {
                return null;
            }
        }
    }
}
