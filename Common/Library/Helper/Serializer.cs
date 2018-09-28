using System;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SunRise.CyberLock.Common.Library.Data;

namespace SunRise.CyberLock.Common.Library.Helper
{
    public static class Serializer
    {
        #region XML serializer
        public static void SaveToXml<T>(this T obj, String directoryPath, String fileName)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            XmlSerializer ser = new XmlSerializer(typeof(T));
            TextWriter writer = new StreamWriter(directoryPath + @"\" + fileName);
            ser.Serialize(writer, obj);
            writer.Close();
        }

        public static object LoadFromXML<T>(String file)
        {
            if (File.Exists(file))
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                TextReader reader = new StreamReader(file);
                var res = ser.Deserialize(reader);
                reader.Close();
                return res;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Binnary serializer
        public static void SaveToBinnary<T>(this T obj, String directoryPath, String fileName)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            FileStream fs = File.Create(directoryPath + @"\" + fileName);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fs, obj);
            fs.Close();
        }

        public static object LoadFromBinnary<T>(String file)
        {
            if (File.Exists(file))
            {
                FileStream fs = File.Open(file, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(fs);
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}
