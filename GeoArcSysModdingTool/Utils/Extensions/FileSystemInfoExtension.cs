using System.IO;
using System.Windows.Media;
using ArcSysAPI.Models;

namespace GeoArcSysModdingTool.Utils.Extensions
{
    public static class FileSystemInfoExtension
    {
        private static readonly ImageSource unknownIconSmall = IconTools.Extract("shell32.dll", 0).ToImageSource();

        private static readonly ImageSource
            unknownIconLarge = IconTools.Extract("shell32.dll", 0, true).ToImageSource();

        public static ImageSource GetIcon(this FileSystemInfo fileSystemInfo, string path = null,
            bool largeIcon = false)
        {
            if (fileSystemInfo is VirtualFileSystemInfo)
                return GetIcon((VirtualFileSystemInfo) fileSystemInfo, largeIcon);

            var icon = IconTools.GetIcon(string.IsNullOrWhiteSpace(path) ? fileSystemInfo.FullName : path,
                largeIcon ? IconTools.SHGFI.LargeIcon : IconTools.SHGFI.SmallIcon);

            if (icon == null)
                icon = IconTools.GetIconForExtension(fileSystemInfo.Extension, largeIcon);

            if (icon == null && fileSystemInfo is DirectoryInfo)
                icon = IconTools.Extract("shell32.dll", 3, largeIcon);

            return icon.ToImageSource();
        }

        private static ImageSource GetIcon(this VirtualFileSystemInfo virtualFileSystemInfo, bool largeIcon = false)
        {
            var icon = IconTools.GetIconForExtension(virtualFileSystemInfo.Extension, largeIcon);
            if (icon != null)
                return icon.ToImageSource();
            return largeIcon ? unknownIconLarge : unknownIconSmall;
        }

        public static ImageSource GetIcon(this DriveInfo driveInfo, bool largeIcon = false)
        {
            var icon = IconTools.GetIcon(driveInfo.RootDirectory.FullName,
                largeIcon ? IconTools.SHGFI.LargeIcon : IconTools.SHGFI.SmallIcon);

            return icon.ToImageSource();
        }
    }
}