using System;
using SunRise.CyberLock.Common.Library.Logger;

namespace SunRise.CyberLock.ClientSide.Settings.SLib
{
    [Serializable]
    public class SettingsFields
    {
        public String App_Folder;
        public String Service_Name;
        public String Service_Port;
        public String Service_Ip;

        public LogLevel Log_Level;

        public int RemTimer_Width;
        public int RemTimer_Height;
        public int RemTimer_FontSize;
        public int RemTimer_RefreshDelay;

        public int KP_TimeWaiting;
        public string[] KP_ExlusionsList;
        public string[] KP_IncludesList;
    }
}
