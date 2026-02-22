#nullable enable
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using net.puk06.ColorChanger.Editor.Models;
using net.puk06.ColorChanger.Editor.Utils;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace net.puk06.ColorChanger.Editor.Services
{
    [InitializeOnLoad]
    internal static class UpdateChecker
    {
        private static string _currentVersion = "";
        private static string _latestVersion = "";

        private const string PackageName = "net.puk06.color-changer";
        private const string UpdateCheckURL = "https://update.pukosrv.net/check/colorchangerunity";
        private static readonly HttpClient _httpClient = new HttpClient();

        static UpdateChecker()
        {
            CheckUpdate();
        }

        private static async void CheckUpdate()
        {
            try
            {
                var packageInfomation = GetPackageInfo(PackageName);
                if (packageInfomation == null) return;

                var version = packageInfomation.version;
                if (version == "") return;

                _currentVersion = version;

                string response = await _httpClient.GetStringAsync(UpdateCheckURL);
                if (response == null) return;

                VersionRelease versionRelease = JsonUtility.FromJson<VersionRelease>(response);
                if (versionRelease == null) return;

                _latestVersion = versionRelease.LatestVersion;

                await Task.Delay(8000);

                if (versionRelease.LatestVersion == version)
                {
                    LogUtils.Log("Your Color Changer is up to date! Thank you for using this!");
                }
                else
                {
                    LogUtils.Log(
                        $"Update available: v{version} → v{versionRelease.LatestVersion}\n" +
                        string.Join("\n", versionRelease.ChangeLog.Select(log => $"・{log}"))
                    );
                }
            }
            catch (Exception ex)
            {
                LogUtils.LogError($"An error occurred while retrieving update information. This may be due to your network connection or server issues.\n{ex}");
            }
        }

        /// <summary>
        /// パッケージ名からパッケージ情報を取得します。
        /// 元コード: https://qiita.com/from2001vr/items/dc0154969b9e1c2f14fd
        /// </summary>
        internal static UnityEditor.PackageManager.PackageInfo? GetPackageInfo(string packageName)
        {
            var request = Client.List(true, true);
            while (!request.IsCompleted) { } // リクエストが終わるまで待機
            if (request.Status == StatusCode.Success)
            {
                return request.Result.FirstOrDefault(pkg => pkg.name == packageName);
            }

            return null;
        }

        /// <summary>
        /// バージョン表記をUI上に作成します
        /// </summary>
        internal static void GenerateVersionLabel()
        {
            if (_currentVersion == "") return;

            GUIStyle versionLabel = new GUIStyle(EditorStyles.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = _currentVersion == "" || _latestVersion == "" || _currentVersion == _latestVersion ? Color.white : Color.yellow }
            };

            if (_currentVersion == "")
            {
                EditorGUILayout.LabelField(
                    "Version",
                    $"Loading...",
                    versionLabel
                );
            }
            else if (_latestVersion == "" || _currentVersion == _latestVersion)
            {
                EditorGUILayout.LabelField(
                    "Version",
                    $"v{_currentVersion}",
                    versionLabel
                );
            }
            else
            {
                EditorGUILayout.LabelField(
                    "Version",
                    $"v{_currentVersion} → v{_latestVersion}",
                    versionLabel
                );
            }
        }
    }
}
