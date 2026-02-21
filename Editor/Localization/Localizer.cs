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
                        _map[deserializeResult["LanguageName"]] = deserializeResult;
                        _languageMetaData.Add((deserializeResult["LanguageName"], deserializeResult["LocalizedLanguageName"]));
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load language: '{Path.GetFileName(filePath)}'.\n{ex}");
                }
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
