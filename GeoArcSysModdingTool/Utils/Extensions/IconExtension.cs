using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GeoArcSysModdingTool.Utils.Extensions
{
    public static class IconExtension
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        private static readonly ImageSource DefaultIcon = IconTools.Extract("shell32.dll", 0, true).ToImageSource();

        public static ImageSource ToImageSource(this Icon icon)
        {
            if (icon == null)
                return DefaultIcon;

            try
            {
                var bitmap = icon.ToBitmap();
                var hBitmap = bitmap.GetHbitmap();

                ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                if (!DeleteObject(hBitmap)) throw new Win32Exception();

                return wpfBitmap;
            }
            catch
            {
                return DefaultIcon;
            }
        }
    }
}