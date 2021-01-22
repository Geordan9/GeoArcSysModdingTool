using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace GeoArcSysModdingTool.Utils
{
    public static class IconTools
    {
        public static Icon Extract(string file, int number, bool largeIcon = false)
        {
            IntPtr large;
            IntPtr small;
            ExtractIconEx(file, number, out large, out small, 1);
            try
            {
                return Icon.FromHandle(largeIcon ? large : small);
            }
            catch
            {
                return null;
            }
        }

        [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion,
            out IntPtr piSmallVersion, int amountIcons);

        //Import SHGetFileInfo function
        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi,
            uint cbSizeFileInfo, uint uFlags);


        [DllImport("Shell32.dll")]
        public static extern int ExtractIconEx(
            string libName,
            int iconIndex,
            IntPtr[] largeIcon,
            IntPtr[] smallIcon,
            uint nIcons
        );

        [DllImport("User32.dll")]
        private static extern int DestroyIcon(IntPtr hIcon);

        //Struct used by SHGetFileInfo function
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public readonly IntPtr hIcon;
            public readonly int iIcon;
            public readonly uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public readonly string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public readonly string szTypeName;
        }

        [Flags]
        public enum SHGFI : uint
        {
            /// <summary>get icon</summary>
            Icon = 0x000000100,

            /// <summary>get display name</summary>
            DisplayName = 0x000000200,

            /// <summary>get type name</summary>
            TypeName = 0x000000400,

            /// <summary>get attributes</summary>
            Attributes = 0x000000800,

            /// <summary>get icon location</summary>
            IconLocation = 0x000001000,

            /// <summary>return exe type</summary>
            ExeType = 0x000002000,

            /// <summary>get system icon index</summary>
            SysIconIndex = 0x000004000,

            /// <summary>put a link overlay on icon</summary>
            LinkOverlay = 0x000008000,

            /// <summary>show icon in selected state</summary>
            Selected = 0x000010000,

            /// <summary>get only specified attributes</summary>
            Attr_Specified = 0x000020000,

            /// <summary>get large icon</summary>
            LargeIcon = 0x000000000,

            /// <summary>get small icon</summary>
            SmallIcon = 0x000000001,

            /// <summary>get open icon</summary>
            OpenIcon = 0x000000002,

            /// <summary>get shell size icon</summary>
            ShellIconSize = 0x000000004,

            /// <summary>pszPath is a pidl</summary>
            PIDL = 0x000000008,

            /// <summary>use passed dwFileAttribute</summary>
            UseFileAttributes = 0x000000010,

            /// <summary>apply the appropriate overlays</summary>
            AddOverlays = 0x000000020,

            /// <summary>Get the index of the overlay in the upper 8 bits of the iIcon</summary>
            OverlayIndex = 0x000000040
        }

        public static Icon GetIcon(string fileName, SHGFI flags)
        {
            var shinfo = new SHFILEINFO();

            SHGetFileInfo(fileName.TrimEnd(Path.DirectorySeparatorChar), 0, ref shinfo, (uint) Marshal.SizeOf(shinfo),
                (uint) (SHGFI.Icon | flags));

            if (shinfo.hIcon == IntPtr.Zero) return null;

            var icon = (Icon) Icon.FromHandle(shinfo.hIcon).Clone();
            DestroyIcon(shinfo.hIcon);
            return icon;
        }

        public static Icon GetIconForExtension(string extension, bool largeIcon = false)
        {
            var keyForExt = Registry.ClassesRoot.OpenSubKey(extension);

            if (keyForExt == null) return null;


            var className = Convert.ToString(keyForExt.GetValue(null));

            var keyForClass = Registry.ClassesRoot.OpenSubKey(className);

            if (keyForClass == null) return null;


            var keyForIcon = keyForClass.OpenSubKey("DefaultIcon");

            if (keyForIcon == null)
            {
                var keyForCLSID = keyForClass.OpenSubKey("CLSID");

                if (keyForCLSID == null) return null;


                var clsid = "CLSID\\"
                            + Convert.ToString(keyForCLSID.GetValue(null))
                            + "\\DefaultIcon";

                keyForIcon = Registry.ClassesRoot.OpenSubKey(clsid);

                if (keyForIcon == null) return null;
            }


            var defaultIcon = Convert.ToString(keyForIcon.GetValue(null)).Split(',');

            var index = defaultIcon.Length > 1
                ? int.Parse(Regex.Match(defaultIcon[1], @"-?\d+").Value)
                : 0;


            var handles = new IntPtr[1];

            if (ExtractIconEx(defaultIcon[0], index,
                largeIcon ? handles : null,
                !largeIcon ? handles : null, 1) > 0)

                return Icon.FromHandle(handles[0]);

            return null;
        }
    }
}