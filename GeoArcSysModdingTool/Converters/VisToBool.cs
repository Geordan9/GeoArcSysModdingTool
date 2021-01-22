using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GeoArcSysModdingTool.Converters
{
    public class VisToBool : IValueConverter
    {
        public bool Inverted { get; set; } = false;

        public bool Not { get; set; } = false;

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return Inverted
                ? BoolToVisibility(value)
                : VisibilityToBool(value);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return Inverted
                ? VisibilityToBool(value)
                : BoolToVisibility(value);
        }

        private object VisibilityToBool(object value)
        {
            if (!(value is Visibility))
                return DependencyProperty.UnsetValue;

            return ((Visibility) value == Visibility.Visible) ^ Not;
        }

        private object BoolToVisibility(object value)
        {
            if (!(value is bool))
                return DependencyProperty.UnsetValue;

            return (bool) value ^ Not
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}