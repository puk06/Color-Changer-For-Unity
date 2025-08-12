using System.Runtime.CompilerServices;
using UnityEngine;

internal static class LogUtils
{
    private const string LOG_PREFIX = "<color=#f9b7e7>Color Changer</color> > ";

    internal static void Log(string message, [CallerMemberName] string caller = "")
    {
        Debug.Log($"{LOG_PREFIX}[{caller}] {message}");
    }

    internal static void LogWarning(string message, [CallerMemberName] string caller = "")
    {
        Debug.LogWarning($"{LOG_PREFIX}[{caller}] {message}");
    }

    internal static void LogError(string message, [CallerMemberName] string caller = "")
    {
        Debug.LogError($"{LOG_PREFIX}[{caller}] {message}");
    }
}
