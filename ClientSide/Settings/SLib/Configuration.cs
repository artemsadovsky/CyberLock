using System;
using SunRise.CyberLock.Common.Library.Logger;
using SunRise.CyberLock.Common.Library.Helper;

namespace SunRise.CyberLock.ClientSide.Settings.SLib
{
    public class Configuration
    {        
        public SettingsFields FIELDS = new SettingsFields();
        
        public Configuration()
        {
            Load();
        }

        private void GenerateDefaultData()
        {
            FIELDS.App_Folder = AppDomain.CurrentDomain.BaseDirectory;
            FIELDS.Service_Name = "CyberLock";
            FIELDS.Service_Port = "1234";
            FIELDS.Service_Ip = "127.0.0.1";

            FIELDS.Log_Level = LogLevel.All;

            FIELDS.RemTimer_Width = 60;
            FIELDS.RemTimer_Height = 25;
            FIELDS.RemTimer_FontSize = 14;
            FIELDS.RemTimer_RefreshDelay = 100;

            FIELDS.KP_TimeWaiting = 60;
            FIELDS.KP_ExlusionsList = new string[] { "avp", "totalcmd" };
            FIELDS.KP_IncludesList = new string[] { "hl" };
        }

        public void Load()
        {
            var obj = Serializer.LoadFromXML<SettingsFields>(Constants.GetSettingsPath());
            if (obj != null)
                FIELDS = (SettingsFields)obj;
            else
            {
                GenerateDefaultData();
                Save();
            }
        }

        public void Save()
        {
            FIELDS.SaveToXml(Constants.CONFIG_DATA_PATH, Constants.CONFIG_SETTINGS_FILE_NAME);
        }

        public String GetAddress()
        {
            // "net.tcp://ip:port/service_name"
            return ("net.tcp://" + FIELDS.Service_Ip + ":" + FIELDS.Service_Port + "/" + FIELDS.Service_Name);
        }
    }
}
