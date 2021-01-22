using System;
using System.Globalization;
using System.Windows.Data;

namespace GeoArcSysModdingTool.Converters
{
    public class BoolToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? 0 : 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int) value == 0 ? true : false;
        }
    }
}