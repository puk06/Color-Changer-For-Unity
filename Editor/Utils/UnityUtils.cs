using UnityEditor;
using Object = UnityEngine.Object;

namespace net.puk06.ColorChanger.Utils
{
    internal static class UnityUtils
    {
        /// <summary>
        /// UnityのAssets内で、指定されたパスのファイルを開きます。
        /// </summary>
        /// <param name="assetPath"></param>
        internal static void SelectAssetAtPath(string assetPath)
        {
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (obj != null)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
            else
            {
                LogUtils.LogError("Failed to open the file: " + assetPath);
            }
        }
    }
}
