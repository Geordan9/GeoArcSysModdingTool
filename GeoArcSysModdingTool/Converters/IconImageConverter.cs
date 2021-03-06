﻿using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using GeoArcSysModdingTool.Utils.Extensions;

namespace GeoArcSysModdingTool.Converters
{
    public class IconImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var icon = value as Icon;
            return icon != null ? icon.ToImageSource() : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}