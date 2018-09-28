using System;
using System.Windows.Data;

namespace SunRise.CyberLock.ServerSide.UI.Converter
{
    public class SourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((int)value)
            {
                case 0:
                    return new Uri(@"Images/Computer_green.png", UriKind.Relative);
                case 1:
                    return new Uri(@"Images/Computer_red.png", UriKind.Relative);
                case 2:
                    return new Uri(@"Images/Computer_yellow.png", UriKind.Relative);
                case 3:
                    return new Uri(@"Images/Computer_gray.png", UriKind.Relative);
                default:
                    return new Uri(@"Images/Computer_blue.png", UriKind.Relative);
            }
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
