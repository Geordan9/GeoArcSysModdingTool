using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace GeoArcSysModdingTool.Converters
{
    public class SwitchBindingExtension : Binding
    {
        public SwitchBindingExtension()
        {
            Initialize();
        }

        public SwitchBindingExtension(string path)
            : base(path)
        {
            Initialize();
        }

        public SwitchBindingExtension(string path, object valueIfTrue, object valueIfFalse)
            : base(path)
        {
            Initialize();
            ValueIfTrue = valueIfTrue;
            ValueIfFalse = valueIfFalse;
        }

        [ConstructorArgument("valueIfTrue")] public object ValueIfTrue { get; set; }

        [ConstructorArgument("valueIfFalse")] public object ValueIfFalse { get; set; }

        private void Initialize()
        {
            ValueIfTrue = DoNothing;
            ValueIfFalse = DoNothing;
            Converter = new SwitchConverter(this);
        }

        private class SwitchConverter : IValueConverter
        {
            private readonly SwitchBindingExtension _switch;

            public SwitchConverter(SwitchBindingExtension switchExtension)
            {
                _switch = switchExtension;
            }

            #region IValueConverter Members

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                try
                {
                    var b = System.Convert.ToBoolean(value);
                    return b ? _switch.ValueIfTrue : _switch.ValueIfFalse;
                }
                catch
                {
                    return DependencyProperty.UnsetValue;
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return DoNothing;
            }

            #endregion
        }
    }
}