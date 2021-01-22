using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcSysAPI.Models;
using ArcSysAPI.Utils;
using GeoArcSysModdingTool.Components;
using GeoArcSysModdingTool.Models;
using GeoArcSysModdingTool.Utils.Extensions;
using Shell32;

namespace GeoArcSysModdingTool.Utils
{
    public static class ExploreTools
    {
        public static List<DirectoryItemMenuItem> DirectoryItemMenuItems;

        public static List<DirectoryItemMenuItem> DirectoryMenuItems;

        public static List<DirectoryItemMenuItem> VirtualFileMenuItems;

        public static List<DirectoryItemMenuItem> PACFileMenuItems;

        public static List<DirectoryItemMenuItem> DynHIPFileMenuItems;

        public static CancellationTokenSource LoadingFilesTaskTokenSource = new CancellationTokenSource();

        public static bool PreemptFileAnalysis;

        static ExploreTools()
        {
            OpenDirectoryInExplorerCommand = new Command<DirectoryItem>(di =>
            {
                var path = di.Path;
                if (!File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                    path = Path.GetDirectoryName(di.Path);

                Process.Start(path);
            });

            DeleteFileCommand = new Command<DirectoryItem>(di =>
            {
                var path = di.Path;
                if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                {
                    di.FSW.Dispose();
                    Directory.Delete(path, true);
                }
                else
                {
                    File.Delete(path);
                }
            });

            ConvertFolderToPACCommand = new Command<DirectoryItem>(di =>
            {
                File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(di.Path), di.Name + ".pac"),
                    new PACFileInfo(di.Path).GetBytes());
            });

            ExportVirtualFileCommand = new Command<DirectoryItem>(di =>
                ExportVirtualFile((VirtualFileSystemInfo) di.Item));

            DeleteVirtualFileCommand = new Command<DirectoryItem>(di =>
                ((VirtualFileSystemInfo) di.Item).Delete());

            ExportPACFilesCommand = new Command<DirectoryItem>(di =>
                ExportPACFiles((PACFileInfo) di.Item));

            ExportHIPPaletteCommand = new Command<DirectoryItem>(di =>
                ExportHIPPalette((HIPFileInfo) di.Item));

            DirectoryItemMenuItems = new List<DirectoryItemMenuItem>
            {
                new DirectoryItemMenuItem
                {
                    Header = "Open in Explorer",
                    Command = OpenDirectoryInExplorerCommand
                },
                new DirectoryItemMenuItem
                {
                    Header = "Delete",
                    Command = DeleteFileCommand
                }
            };

            DirectoryMenuItems = new List<DirectoryItemMenuItem>
            {
                new DirectoryItemMenuItem
                {
                    Header = "Convert To",
                    ChildMenuItems = new[]
                    {
                        new DirectoryItemMenuItem
                        {
                            Header = "PAC",
                            Command = ConvertFolderToPACCommand
                        }
                    }
                }
            };

            VirtualFileMenuItems = new List<DirectoryItemMenuItem>
            {
                new DirectoryItemMenuItem
                {
                    Header = "Export",
                    Command = ExportVirtualFileCommand
                },
                new DirectoryItemMenuItem
                {
                    Header = "Delete",
                    Command = DeleteVirtualFileCommand
                }
            };

            PACFileMenuItems = new List<DirectoryItemMenuItem>
            {
                new DirectoryItemMenuItem
                {
                    Header = "Export Files",
                    Command = ExportPACFilesCommand
                }
            };

            DynHIPFileMenuItems = new List<DirectoryItemMenuItem>
            {
                new DirectoryItemMenuItem
                {
                    Header = "Export Palette",
                    Command = ExportHIPPaletteCommand
                }
            };
        }

        public static ICommand OpenDirectoryInExplorerCommand { get; set; }

        public static ICommand DeleteFileCommand { get; set; }

        public static ICommand ConvertFolderToPACCommand { get; set; }

        public static ICommand ExportVirtualFileCommand { get; set; }

        public static ICommand DeleteVirtualFileCommand { get; set; }

        public static ICommand ExportPACFilesCommand { get; set; }

        public static ICommand ExportHIPPaletteCommand { get; set; }

        private static void ExportPACFiles(PACFileInfo pacFileInfo)
        {
            ChangeWindowOpacity(0.6);
            var wasActive = pacFileInfo.Active;
            try
            {
                var path = Path.Combine(Dialogs.OpenFolderDialog("Export files in..."),
                    Path.GetFileNameWithoutExtension(pacFileInfo.Name));
                if (string.IsNullOrWhiteSpace(path))
                    return;

                Directory.CreateDirectory(path);

                pacFileInfo.Active = true;

                foreach (var file in pacFileInfo.GetFiles())
                    File.WriteAllBytes(Path.Combine(path, file.Name), file.GetBytes());
            }
            finally
            {
                if (!wasActive)
                    pacFileInfo.Active = false;
                ChangeWindowOpacity(1.0);
            }
        }

        private static void ExportVirtualFile(VirtualFileSystemInfo virtualFile)
        {
            ChangeWindowOpacity(0.6);
            try
            {
                var path = Dialogs.SaveFileDialog("Export As...",
                    $"{new string(virtualFile.Extension.Skip(1).ToArray()).ToUpper()} File|*{virtualFile.Extension}",
                    virtualFile.Name);
                if (string.IsNullOrWhiteSpace(path))
                    return;

                File.WriteAllBytes(path, virtualFile.GetBytes());
            }
            finally
            {
                ChangeWindowOpacity(1.0);
            }
        }

        private static void ExportHIPPalette(HIPFileInfo hipFileInfo)
        {
            ChangeWindowOpacity(0.6);
            try
            {
                var path = Dialogs.SaveFileDialog("Export As...",
                    "HPL File|*.hpl|ACT File|*.act",
                    Path.GetFileNameWithoutExtension(hipFileInfo.Name));
                if (string.IsNullOrWhiteSpace(path))
                    return;

                var colors = hipFileInfo.GetPalette().ToMediaColors();
                var extension = Path.GetExtension(path);
                byte[] fileBytes;
                switch (extension)
                {
                    case ".hpl":
                        fileBytes = PaletteTools.CreateHPLByteArray(colors);
                        break;
                    case ".act":
                        fileBytes = PaletteTools.CreateACTByteArray(colors);
                        break;
                    default:
                        fileBytes = new byte[0];
                        break;
                }

                if (fileBytes.Length > 0)
                    File.WriteAllBytes(path, fileBytes);
            }
            finally
            {
                ChangeWindowOpacity(1.0);
            }
        }

        public static DirectoryItem GetItem(DriveInfo drive)
        {
            var label = string.IsNullOrWhiteSpace(drive.VolumeLabel) ? "Local Disk" : drive.VolumeLabel;
            var di = new DirectoryItem(drive)
            {
                Name = $"{label} ({drive.Name.TrimEnd(Path.DirectorySeparatorChar)})",
                Path = drive.RootDirectory.FullName,
                FSW = CreateFileSystemWatcher(drive.RootDirectory.FullName),
                Items = SetDummy(drive.RootDirectory.FullName)
            };
            di.MenuItems.AddRange(DirectoryItemMenuItems);
            return di;
        }

        public static DirectoryItem GetItem(DirectoryInfo directory)
        {
            var di = new DirectoryItem(directory)
            {
                Path = directory.FullName,
                FSW = CreateFileSystemWatcher(directory.FullName),
                Items = SetDummy(directory.FullName)
            };
            di.MenuItems.AddRange(DirectoryItemMenuItems);
            di.MenuItems.AddRange(DirectoryMenuItems);
            return di;
        }

        public static DirectoryItem GetItem(FileInfo file)
        {
            var di = new DirectoryItem(file)
            {
                Path = file.FullName,
                Item = file
            };

            if (file.Extension == ".lnk")
            {
                var target = GetLnkTarget(file.FullName);
                var attr = File.GetAttributes(target);
                DirectoryItem directoryItem;
                if (attr.HasFlag(FileAttributes.Directory))
                    directoryItem = GetItem(new DirectoryInfo(GetLnkTarget(file.FullName)));
                else
                    directoryItem = GetItem(new FileInfo(GetLnkTarget(file.FullName)));
                di.Name = di.Name.Substring(0, di.Name.LastIndexOf('.'));
                di.Item = directoryItem.Item;
                di.Items = directoryItem.Items;
                di.MenuItems.AddRange(directoryItem.MenuItems);
            }

            if (file.Extension == ".pac" ||
                file.Extension == ".paccs" ||
                file.Extension == ".pacgz" ||
                file.Extension == ".fontpac")
            {
                var pfi = new PACFileInfo(file.FullName, PreemptFileAnalysis);
                if (pfi.IsValidPAC || !PreemptFileAnalysis)
                    return GetItem(pfi);
            }

            if (string.IsNullOrWhiteSpace(file.Extension))
            {
                if (MD5Tools.IsMD5(file.Name)) return GetItem(new PACFileInfo(file.FullName, PreemptFileAnalysis));
                var length = file.Name.LastIndexOf('_');
                if (length >= 32)
                {
                    var name = file.Name.Substring(0, length);
                    if (MD5Tools.IsMD5(name))
                    {
                        var pfi = new PACFileInfo(file.FullName, PreemptFileAnalysis);

                        if (pfi.IsValidPAC || !PreemptFileAnalysis)
                            return GetItem(pfi);
                    }
                }
            }

            if (file.Extension == ".hip")
            {
                var hipFileInfo = new HIPFileInfo(file.FullName, PreemptFileAnalysis);

                if (hipFileInfo.IsValidHIP || !PreemptFileAnalysis)
                    return GetItem(hipFileInfo);
            }

            if (file.Extension == ".dds")
            {
                var ddsFileInfo = new DDSFileInfo(file.FullName, PreemptFileAnalysis);

                if (ddsFileInfo.IsValidDDS || !PreemptFileAnalysis)
                    return GetItem(ddsFileInfo);
            }

            di.MenuItems.AddRange(DirectoryItemMenuItems);

            return di;
        }

        public static DirectoryItem GetItem(VirtualFileSystemInfo virtualFile)
        {
            var di = new DirectoryItem(virtualFile)
            {
                Path = virtualFile.FullName,
                Item = virtualFile
            };
            if (virtualFile is VirtualDirectoryInfo)
            {
                di.Items = SetDummy((VirtualDirectoryInfo) virtualFile);
                di.FSW = CreateFileSystemWatcher(new FileInfo(virtualFile.VirtualRoot.FullName).Directory.FullName);
            }

            if (virtualFile.Extension == ".pac" ||
                virtualFile.Extension == ".paccs" ||
                virtualFile.Extension == ".pacgz" ||
                virtualFile.Extension == ".fontpac")
            {
                var pfi = di.Item as PACFileInfo;
                if (pfi.IsValidPAC || !PreemptFileAnalysis)
                    di.Item = pfi;
            }
            else if (virtualFile.Extension == ".hip")
            {
                var hipFileInfo = di.Item as HIPFileInfo;
                if (hipFileInfo.IsValidHIP || !PreemptFileAnalysis)
                    di.Item = hipFileInfo;
            }

            if (di.Item is PACFileInfo)
                di.MenuItems.AddRange(PACFileMenuItems);

            if (virtualFile.Parent != null)
                di.MenuItems.AddRange(VirtualFileMenuItems);
            else
                di.MenuItems.AddRange(DirectoryItemMenuItems);

            return di;
        }

        private static bool IsDirectoryEmpty(string path)
        {
            try
            {
                return !Directory.EnumerateFileSystemEntries(path).Any();
            }
            catch
            {
                return true;
            }
        }

        public static ObservableRangeCollection<DirectoryItem> SetDummy(string path)
        {
            return IsDirectoryEmpty(path)
                ? new ObservableRangeCollection<DirectoryItem>()
                : new ObservableRangeCollection<DirectoryItem> {new DirectoryItemDummy()};
        }

        public static ObservableRangeCollection<DirectoryItem> SetDummy(FileSystemInfo fsi)
        {
            if (fsi is PACFileInfo)
                if (((PACFileInfo) fsi).FileCount > 0 || !PreemptFileAnalysis)
                    return new ObservableRangeCollection<DirectoryItem> {new DirectoryItemDummy()};
            if (fsi is DirectoryInfo)
                return SetDummy(((DirectoryInfo) fsi).FullName);
            return new ObservableRangeCollection<DirectoryItem>();
        }

        public static DirectoryInfo GetDirectoryInfo(DirectoryItem item)
        {
            var directoryInfo = (DirectoryInfo) null;
            var Item = item.Item;
            if (Item is DriveInfo)
                directoryInfo = ((DriveInfo) Item).RootDirectory;
            else if (Item is DirectoryInfo)
                directoryInfo = (DirectoryInfo) Item;
            else if (Item is FileInfo) directoryInfo = ((FileInfo) Item).Directory;
            return directoryInfo;
        }

        public static DirectoryItem[] ExploreDirectories(DirectoryItem item)
        {
            var fileList = new List<DirectoryItem>();
            var directoryInfo = GetDirectoryInfo(item);
            if (ReferenceEquals(directoryInfo, null)) return fileList.ToArray();
            foreach (var directory in directoryInfo.EnumerateDirectories())
            {
                var isHidden = (directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                var isSystem = (directory.Attributes & FileAttributes.System) == FileAttributes.System;
                if (!isHidden && !isSystem) fileList.Add(GetItem(directory));
                if (LoadingFilesTaskTokenSource.IsCancellationRequested) return fileList.ToArray();
            }

            return fileList.ToArray();
        }

        public static DirectoryItem[] ExploreFiles(DirectoryItem item)
        {
            var fileList = new ConcurrentQueue<DirectoryItem>();
            var directoryInfo = GetDirectoryInfo(item);
            if (ReferenceEquals(directoryInfo, null)) return fileList.ToArray();

            Parallel.ForEach(directoryInfo.EnumerateFiles(), new ParallelOptions
                {
                    MaxDegreeOfParallelism = 3,
                    CancellationToken = LoadingFilesTaskTokenSource.Token
                },
                (file, loopstate) =>
                {
                    var isHidden = (file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                    var isSystem = (file.Attributes & FileAttributes.System) == FileAttributes.System;
                    if (!isHidden && !isSystem) fileList.Enqueue(GetItem(file));
                    if (LoadingFilesTaskTokenSource.IsCancellationRequested) loopstate.Break();
                });

            var array = fileList.ToArray();
            Array.Sort(array, (x, y) => string.Compare(x.Name, y.Name));
            return array;
        }

        public static DirectoryItem[] ExplorePACFiles(DirectoryItem item)
        {
            var fileList = new List<DirectoryItem>();
            var pacFileInfo = (PACFileInfo) item.Item;
            if (pacFileInfo.Files == null)
                pacFileInfo.GetFiles();
            if (pacFileInfo.Files != null)
                foreach (var file in pacFileInfo.Files)
                {
                    fileList.Add(GetItem(file));
                    if (LoadingFilesTaskTokenSource.IsCancellationRequested) return fileList.ToArray();
                }

            return fileList.ToArray();
        }

        private static FileSystemWatcher CreateFileSystemWatcher(string path)
        {
            var fsw = new FileSystemWatcher(path);
            fsw.NotifyFilter = NotifyFilters.LastAccess
                               | NotifyFilters.LastWrite
                               | NotifyFilters.FileName
                               | NotifyFilters.DirectoryName;

            /*fsw.Changed += OnChanged;
            fsw.Created += OnChanged;
            fsw.Deleted += OnChanged;
            fsw.Renamed += OnRenamed;*/

            fsw.EnableRaisingEvents = true;

            return fsw;
        }

        private static string GetLnkTarget(string lnkPath)
        {
            lnkPath = Path.GetFullPath(lnkPath);
            var dir = GetShell32NameSpaceFolder(Path.GetDirectoryName(lnkPath));
            var itm = dir.Items().Item(Path.GetFileName(lnkPath));
            var lnk = (ShellLinkObject) itm.GetLink;
            return lnk.Target.Path;
        }

        private static Folder GetShell32NameSpaceFolder(object folder)
        {
            var shellAppType = Type.GetTypeFromProgID("Shell.Application");

            var shell = Activator.CreateInstance(shellAppType);
            return (Folder) shellAppType.InvokeMember("NameSpace",
                BindingFlags.InvokeMethod, null, shell, new[] {folder});
        }

        private static void ChangeWindowOpacity(double opacity)
        {
            Mediator.NotifyColleagues("ChangeWindowOpacity", opacity);
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            var test = source;
        }
    }
}