using System;

namespace SunRise.CyberLock.ClientSide.Settings.SLib
{
    public static class Constants
    {
        public static readonly String CONFIG_DATA_PATH = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                                                                    + @"\SunRise\CyberLock-Client";

        public static readonly String CONFIG_LOG_PATH = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                                                                    + @"\SunRise\CyberLock-Client\logs";

        public static readonly String CONFIG_SESSION_FILE_NAME = "session.xml";
        public static readonly String CONFIG_SETTINGS_FILE_NAME = "config.xml";

        public static readonly String FULL_LOG_NAME = "app-full.log";
        public static readonly String HOST_LOG_NAME = "host.log";

        public static readonly String INSPECTOR_FILE_NAME = "Inspector.exe";
        public static readonly String INSPECTOR_APP_NAME = "Inspector";


        public static String GetSessionPath()
        {
            return CONFIG_DATA_PATH + @"\" + CONFIG_SESSION_FILE_NAME;
        }

        public static String GetSettingsPath()
        {
            return CONFIG_DATA_PATH + @"\" + CONFIG_SETTINGS_FILE_NAME;
        }
    }
}
