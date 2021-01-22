using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using ArcSysAPI.Models;
using GeoArcSysModdingTool.Models;
using GeoArcSysModdingTool.Utils;
using GeoArcSysModdingTool.Utils.Extensions;
using Microsoft.WindowsAPICodePack.Shell;

namespace GeoArcSysModdingTool.Converters
{
    public class DirItemToThumbnailConverter : IMultiValueConverter
    {
        public bool Large { get; set; } = false;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var dirItem = (DirectoryItem) values[0];
            return GetThumbnail(dirItem, Large);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("DirItemToThumbnailConverter can only be used OneWay.");
        }

        private static readonly ImageSource defaultIcon = IconTools.Extract("shell32.dll", 0, true).ToImageSource();

        private ImageSource GetThumbnail(DirectoryItem dirItem, bool largeIcon = false)
        {
            largeIcon = false;
            ImageSource image = null;
            try
            {
                if (dirItem != null)
                {
                    if (!dirItem.ShowThumbnail || !largeIcon)
                    {
                        if (dirItem.ShellIconIndex != 0)
                            image = IconTools.Extract("shell32.dll", dirItem.ShellIconIndex, largeIcon).ToImageSource();
                        else if (dirItem.Item is DriveInfo)
                            image = ((DriveInfo) dirItem.Item).GetIcon(largeIcon);
                        else if (dirItem.Item is FileSystemInfo)
                            image = ((FileSystemInfo) dirItem.Item).GetIcon(dirItem.Path, largeIcon);
                    }
                    else if (largeIcon)
                    {
                        if (dirItem.Item is FileInfo)
                        {
                            var thumbnail = ShellObject.FromParsingName(dirItem.Path).Thumbnail.LargeBitmapSource;
                            if (image != thumbnail)
                                image = thumbnail;
                        }
                        else if (dirItem.Item is HIPFileInfo)
                        {
                            var hipFile = (HIPFileInfo) dirItem.Item;
                            if (largeIcon && hipFile.Obfuscation != VirtualFileSystemInfo.FileObfuscation.None)
                                image = null;
                            else
                                image = hipFile.GetImage().ImageSource();
                        }
                        else if (dirItem.Item is DDSFileInfo)
                        {
                            var ddsFileInfo = (DDSFileInfo) dirItem.Item;
                            if (largeIcon && ddsFileInfo.Obfuscation != VirtualFileSystemInfo.FileObfuscation.None)
                                image = null;
                            else
                                image = ddsFileInfo.GetImage().ImageSource();
                        }
                    }
                }

                if (image == null)
                    image = defaultIcon;
            }
            catch
            {
                image = defaultIcon;
            }

            image.Freeze();
            return image;
        }
    }
}