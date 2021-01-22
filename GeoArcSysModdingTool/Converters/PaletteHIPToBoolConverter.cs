using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Data;
using ArcSysAPI.Models;

namespace GeoArcSysModdingTool.Converters
{
    public class PaletteHIPToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            var hipFileInfo = value as HIPFileInfo;
            if (hipFileInfo == null)
                return false;

            var palette = hipFileInfo.Palette;

            return hipFileInfo.PixelFormat == PixelFormat.Format8bppIndexed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}