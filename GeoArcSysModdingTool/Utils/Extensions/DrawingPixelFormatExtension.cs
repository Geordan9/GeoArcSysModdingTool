using System.Windows.Media;

namespace GeoArcSysModdingTool.Utils.Extensions
{
    public static class DrawingPixelFormatExtension
    {
        public static PixelFormat ToMediaFormat(this System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:
                    return PixelFormats.Gray16;
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:
                    return PixelFormats.Bgr555;
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                    return PixelFormats.Bgr565;

                case System.Drawing.Imaging.PixelFormat.Indexed:
                    return PixelFormats.Bgr101010;
                case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                    return PixelFormats.Indexed1;
                case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                    return PixelFormats.Indexed4;
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    return PixelFormats.Indexed8;

                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return PixelFormats.Bgr24;

                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return PixelFormats.Bgra32;
                case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
                    return PixelFormats.Pbgra32;
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return PixelFormats.Bgr32;

                case System.Drawing.Imaging.PixelFormat.Format48bppRgb:
                    return PixelFormats.Rgb48;

                case System.Drawing.Imaging.PixelFormat.Format64bppArgb:
                    return PixelFormats.Prgba64;

                case System.Drawing.Imaging.PixelFormat.Undefined:
                    return PixelFormats.Default;
            }

            return new PixelFormat();
        }
    }
}