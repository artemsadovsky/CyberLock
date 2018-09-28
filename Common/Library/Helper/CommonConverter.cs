using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunRise.CyberLock.Common.Library.Helper
{
    public static class CommonConverter
    {
        public static DateTime ToDateTime(this String value)
        {
            DateTime resultTime = DateTime.Now;
            try
            {
                string format = "HH:mm:ss";
                resultTime = DateTime.ParseExact(value, format, null);
            }
            catch (FormatException)
            {
                //Unable to convert {date} to a date.
            }
            return resultTime;
        }

        public static String ToString(this DateTime value)
        {
            String resultTime = "";
            try
            {
                string format = "HH:mm:ss";
                resultTime = value.ToString(format);
            }
            catch (FormatException)
            {
                //Unable to convert {date} to a date.
            }
            return resultTime;
        }

        public static Double ToDouble(this String value)
        {
            try
            {
                return System.Convert.ToDouble(value);
            }
            catch (FormatException)
            {
                //Unable to convert {date} to a date.
            }
            return -1;
        }
    }
}
