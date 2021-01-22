using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GeoArcSysModdingTool.Converters
{
    public class ExtensionToUpperConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ext = (string) value;
            return ext.Substring(ext.Length & 1).ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}