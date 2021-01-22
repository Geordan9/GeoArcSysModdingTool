using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GeoArcSysModdingTool.Utils.Extensions
{
    public static class BitmapSourceExtension
    {
        public static void SaveImageAs(this BitmapSource bmpsrc, string fileName = "")
        {
            var path = Dialogs.SaveFileDialog("Save As...",
                "PNG File|*.png|" +
                "JPG File|*.jpg;*.jpeg;*.jpe;*.jfif|" +
                "BMP File|*.bmp;*.dib|" +
                "TIFF File|*.tif;*.tiff|" +
                "GIF File|*.gif|" +
                "WMP File|*.wmp",
                fileName);
            if (string.IsNullOrWhiteSpace(path))
                return;
            var ext = Path.GetExtension(path).ToLower();
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                BitmapEncoder bitmapEncoder = null;
                switch (ext)
                {
                    case ".gif":
                        bitmapEncoder = new GifBitmapEncoder();
                        break;
                    case ".jpg":
                    case ".jpeg":
                    case ".jpe":
                    case ".jfif":
                        bitmapEncoder = new BmpBitmapEncoder();
                        break;
                    case ".png":
                        bitmapEncoder = new PngBitmapEncoder();
                        break;
                    case ".tif":
                    case ".tiff":
                        bitmapEncoder = new TiffBitmapEncoder();
                        break;
                    case ".wmp":
                        bitmapEncoder = new WmpBitmapEncoder();
                        break;
                    default:
                        bitmapEncoder = new BmpBitmapEncoder();
                        break;
                }

                try
                {
                    bitmapEncoder.Frames.Add(BitmapFrame.Create(bmpsrc, null, null, null));
                }
                catch (NotSupportedException)
                {
                }

                bitmapEncoder.Save(fileStream);
                fileStream.Close();
            }
        }

        public static BitmapSource ChangePalette(this BitmapSource bmpsrc, BitmapPalette bmpplt)
        {
            var pixels = new byte[bmpsrc.PixelWidth * bmpsrc.PixelHeight];
            bmpsrc.CopyPixels(pixels, bmpsrc.PixelWidth, 0);
            var writeableBitmap = new WriteableBitmap(bmpsrc.PixelWidth, bmpsrc.PixelHeight, bmpsrc.DpiX, bmpsrc.DpiY,
                bmpsrc.Format, bmpplt);
            writeableBitmap.WritePixels(new Int32Rect(0, 0, bmpsrc.PixelWidth, bmpsrc.PixelHeight),
                pixels, bmpsrc.PixelWidth, 0);
            return writeableBitmap;
        }
    }
}