using System;
using System.Windows.Data;
using SunRise.CyberLock.Common.Library.Helper;

namespace SunRise.CyberLock.ServerSide.UI.Converter
{
    public class EndSessionMinuteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return DateTime.Now.AddMinutes((double)value).Minute.ToString();

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((String)value).ToDouble();
        }
    }
}
