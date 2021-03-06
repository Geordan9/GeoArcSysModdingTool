﻿using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace GeoArcSysModdingTool.Converters
{
    public class FormatKbSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var number = System.Convert.ToInt64(value);
            var sb = new StringBuilder(32);
            StrFormatByteSizeW(number, sb, sb.Capacity);
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern long StrFormatByteSizeW(long qdw, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszBuf,
            int cchBuf);
    }
}