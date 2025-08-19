using net.puk06.ColorChanger.Models;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEditor.PackageManager;

namespace net.puk06.ColorChanger.Utils
{
    [InitializeOnLoad]
    public static class UpdateUtils
    {
        private const string PackageName = "net.puk06.color-changer";
        private const string UpdateCheckURL = "https://update.pukosrv.net/check/colorchangerunity";
        private static readonly HttpClient _httpClient = new HttpClient();

        static UpdateUtils()
        {
            CheckUpdate();
        }

        private static async void CheckUpdate()
        {
            try
            {
                // ちょっと待機
                await Task.Delay(8000);

                var packageInfomation = GetPackageInfo(PackageName);
                if (packageInfomation == null) return;

                var version = packageInfomation.version;
                if (version == "") return;

                string response = await _httpClient.GetStringAsync(UpdateCheckURL);
                if (response == null) return;

                VersionData versionData = JsonUtility.FromJson<VersionData>(response);
                if (versionData == null) return;

                if (versionData.LatestVersion == version)
                {
                    LogUtils.Log("Your Color Changer is up to date! Thank you for using this!");
                }
                else
                {
                    LogUtils.Log(
                        $"Update available: v{version} → v{versionData.LatestVersion}\n" +
                        string.Join("\n", versionData.ChangeLog.Select(log => $"・{log}"))
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
        public static UnityEditor.PackageManager.PackageInfo GetPackageInfo(string packageName)
        {
            var request = Client.List(true, true);
            while (!request.IsCompleted) { } // リクエストが終わるまで待機
            if (request.Status == StatusCode.Success)
            {
                return request.Result.FirstOrDefault(pkg => pkg.name == packageName);
            }
            
            return null;
        }
    }
}
