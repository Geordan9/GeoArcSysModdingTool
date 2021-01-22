using System;
using System.Globalization;
using System.Windows.Data;

namespace GeoArcSysModdingTool.Converters
{
    public class ByteToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToByte(value) - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToByte(value) + 1;
        }
    }
}