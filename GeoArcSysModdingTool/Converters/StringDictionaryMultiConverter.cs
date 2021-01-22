using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace GeoArcSysModdingTool.Converters
{
    public class StringDictionaryMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var value = (string) values[0];
            var dictionary = (Dictionary<string, string>) values[1];
            if (dictionary.Keys.Contains(value))
                value = dictionary[value];
            return value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}