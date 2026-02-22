using System;

namespace net.puk06.ColorChanger.Editor.Models
{
    [Serializable]
    public class VersionRelease
    {
        public string LatestVersion = string.Empty;
        public string[] ChangeLog = Array.Empty<string>();
    }
}
