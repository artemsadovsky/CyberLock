using System;
using System.Windows.Data;
using SunRise.CyberLock.Common.Library.Helper;

namespace SunRise.CyberLock.ServerSide.UI.Converter
{
    public class EndSessionHourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return DateTime.Now.AddHours((double)value).Hour.ToString();

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value as String).ToDouble();
        }
    }
}
