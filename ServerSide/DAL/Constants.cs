using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunRise.CyberLock.ServerSide.DAL
{
    public static class Constants
    {
        public static String CONFIG_DIR_PATH =                  Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
                                                                    + @"\SunRise\CyberLock";

        public static String CONFIG_CLIENT_FILE_NAME =          "clients.xml";
        public static String CONFIG_SETTINGS_FILE_NAME =        "config.xml";
        public static String CONFIG_PRICE_NAME =                "price.xml";
        

        public static String GetClientsPath()
        {
            return CONFIG_DIR_PATH + @"\" + CONFIG_CLIENT_FILE_NAME;
        }

        public static String GetSettingsPath()
        {
            return CONFIG_DIR_PATH + @"\" + CONFIG_SETTINGS_FILE_NAME;
        }
        
        public static String GetPricePath()
        {
            return CONFIG_DIR_PATH + @"\" + CONFIG_PRICE_NAME;
        }
    }
}
