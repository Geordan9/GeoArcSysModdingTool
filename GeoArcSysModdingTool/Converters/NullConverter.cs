using System;
using System.Globalization;
using System.Windows.Data;

namespace GeoArcSysModdingTool.Converters
{
    public class NullConverter : IValueConverter
    {
        public bool Not { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null) ^ Not;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }
}