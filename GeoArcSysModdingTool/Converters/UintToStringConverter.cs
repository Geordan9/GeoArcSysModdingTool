using System;
using System.Globalization;
using System.Windows.Data;

namespace GeoArcSysModdingTool.Converters
{
    public class UintToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToUInt32(value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return uint.Parse((string) value);
        }
    }
}