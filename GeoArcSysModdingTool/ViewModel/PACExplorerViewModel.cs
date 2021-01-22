using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArcSysAPI.Models;
using GeoArcSysModdingTool.Components;
using GeoArcSysModdingTool.Models;
using GeoArcSysModdingTool.Utils;
using GeoArcSysModdingTool.Utils.Extensions;
using static GeoArcSysModdingTool.Utils.ExploreTools;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace GeoArcSysModdingTool.ViewModel
{
    public class PACExplorerViewModel : INotifyPropertyChanged
    {
        private readonly SynchronizationContext uiContext = SynchronizationContext.Current;

        private DirectoryItem curDirItem = new DirectoryItemDummy();

        private DirectoryItem ThisPCDirItem = new DirectoryItemDummy();

        private VirtualFileSystemInfo curVirtualFile;

        private DirectoryItem lastDirItem = new DirectoryItemDummy();

        private Color originalBGColor = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
        private PreviewImage previewImage = new PreviewImage();
        private ObservableRangeCollection<DirectoryItem> rootDirItems = new ObservableRangeCollection<DirectoryItem>();
        private CancellationTokenSource LoadingThumbsTaskTokenSource = new CancellationTokenSource();

        private bool transparentBackground;
        private bool willResetZoomBox;
        private bool openInApp;

        private readonly string[] supportedImageExtensions =
        {
            ".bmp", ".dib", ".gif", ".jpeg", ".jpg", ".jpe", ".jfif",
            ".png", ".tiff", ".tif", ".wmp", ".hip", ".dds"
        };

        public PACExplorerViewModel()
        {
            Mediator.Register("ImportPalette", ImportPalette_Mediator);
            Mediator.Register("UpdateContextMenu", UpdateContextMenu_Mediator);
            Mediator.Register("UpdateSettings", UpdateSettings_Mediator);
            Mediator.Register("LoadDirectories", obj => { LoadDirectories(); });
            ExpandCollapseCommand = new Command<object[]>(obj =>
            {
                if ((bool) obj[0])
                    ExploreItem((DirectoryItem) obj[1], false);
                else
                    CollapseItem((DirectoryItem) obj[1]);
            });
            RefreshCommand = new Command(LoadDirectories);
            OpenDirectoryCommand = new Command<DirectoryItem>(di =>
            {
                if (!(di.Item is FileInfo || di.Item is VirtualFileInfo))
                    OpenDirectory(di);
            });
            OpenDirectoryItemCommand = new Command<DirectoryItem>(di => { OpenDirectory(di); });
            SelectImageCommand = new Command<DirectoryItem>(di =>
            {
                if (di.Item is HIPFileInfo || di.Item is DDSFileInfo) OpenDirectory(di);
            });
            GoUpDirCommand = new Command(GoUpDir);
            TransparentBackgroundCommand = new Command(() =>
            {
                UpdateTransparency();
                UpdatePreviewImageData((FileSystemInfo) PreviewImage.Item, true);
            });
            ChangeImageCommand = new Command<string>(str => ChangeImage(bool.Parse(str)));
            ChangePaletteCommand = new Command<string>(str => ChangePalette(byte.Parse(str)));
        }

        // properties

        public static ICommand ExpandCollapseCommand { get; set; }

        public static ICommand RefreshCommand { get; set; }

        public static ICommand OpenDirectoryCommand { get; set; }

        public static ICommand OpenDirectoryItemCommand { get; set; }

        public static ICommand SelectImageCommand { get; set; }

        public static ICommand GoUpDirCommand { get; set; }

        public static ICommand GetPaletteCommand { get; set; }

        public static ICommand TransparentBackgroundCommand { get; set; }

        public static ICommand ChangeImageCommand { get; set; }

        public static ICommand ChangePaletteCommand { get; set; }

        public ObservableRangeCollection<DirectoryItem> RootDirItems
        {
            get => rootDirItems;
            set
            {
                rootDirItems = value;
                OnPropertyChanged();
            }
        }

        public DirectoryItem CurDirItem
        {
            get => curDirItem;
            set
            {
                curDirItem = value;
                OnPropertyChanged();
            }
        }

        public PreviewImage PreviewImage
        {
            get => previewImage;
            set
            {
                previewImage = value;
                OnPropertyChanged();
            }
        }

        public bool WillResetZoomBox
        {
            get => willResetZoomBox;
            set
            {
                willResetZoomBox = value;
                OnPropertyChanged();
            }
        }

        public bool TransparentBackground
        {
            get => transparentBackground;
            set
            {
                transparentBackground = value;
                OnPropertyChanged();
            }
        }

        public bool OpenInApp
        {
            get => openInApp;
            set
            {
                openInApp = value;
                OnPropertyChanged();
            }
        }

        // INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        // methods

        private void LoadDirectories()
        {
            RootDirItems.Clear();

            // Load This PC
            var ThisPC = new DirectoryItem(null, 15)
            {
                Name = "This PC",
                IsExpanded = true,
                IsSpecialFolder = true
            };
            var drives = DriveInfo.GetDrives().Where(x => x.IsReady).ToList();
            foreach (var drive in drives) ThisPC.Items.Add(GetItem(drive));
            ThisPCDirItem = ThisPC;
            RootDirItems.Add(ThisPCDirItem);
            CurDirItem = ThisPCDirItem;
            lastDirItem = ThisPCDirItem;
        }

        private void GoUpDir()
        {
            var vdi = CurDirItem.Item as VirtualDirectoryInfo;
            var dii = CurDirItem.Item as DirectoryInfo;
            var dri = CurDirItem.Item as DriveInfo;
            if (PreviewImage.Item != null || PreviewImage.Source != null)
            {
                PreviewImage.Path = string.Empty;
                PreviewImage.PalettePath = string.Empty;
                PreviewImage.PaletteItem = null;
                PreviewImage.Item = null;
                PreviewImage.Source = null;
            }
            else if (vdi != null || dii != null || dri != null)
            {
                lastDirItem = CurDirItem;
                if (vdi == null && File.Exists(CurDirItem.Path))
                {
                    CurDirItem = GetItem(new FileInfo(CurDirItem.Path).Directory);
                }
                else if (vdi != null)
                {
                    var vfsi = (VirtualFileSystemInfo) CurDirItem.Item;
                    if (vfsi.Parent != null)
                        CurDirItem = GetItem(vfsi.Parent);
                    else
                        CurDirItem = GetItem(new FileInfo(vfsi.GetPrimaryPath()).Directory);
                }
                else if (dii != null)
                {
                    var dir = (DirectoryInfo) CurDirItem.Item;
                    if (dir.FullName == dir.Root.FullName || dir.Parent.FullName == dir.Root.FullName)
                        CurDirItem = GetItem(new DriveInfo(dir.FullName));
                    else
                        CurDirItem = GetItem(dii.Parent);
                }
                else if (dri != null)
                {
                    CurDirItem = ThisPCDirItem;
                    return;
                }

                OpenDirectory(CurDirItem, true);
            }
        }

        private void OpenDirectory(DirectoryItem item, bool wentBack = false)
        {
            var isContainer = item.Item is DirectoryInfo || item.Item is DriveInfo || item.Item is VirtualDirectoryInfo;
            if ((isContainer || item.IsSpecialFolder) && (CurDirItem != item || wentBack))
            {
                if (!lastDirItem.IsExpanded && isContainer && !LoadingFilesTaskTokenSource.IsCancellationRequested)
                {
                    LoadingFilesTaskTokenSource.Cancel();
                    LoadingThumbsTaskTokenSource.Cancel();
                    lastDirItem.Items = item.Item is VirtualDirectoryInfo
                        ? SetDummy((VirtualDirectoryInfo) item.Item)
                        : SetDummy(item.Item is DriveInfo
                            ? ((DriveInfo) item.Item).RootDirectory.FullName
                            : ((DirectoryInfo) item.Item).FullName);
                }

                lastDirItem = CurDirItem;
                ExploreItem(item, true);
                CurDirItem = item;
            }

            SetItemActive(item.Item);

            if (item.Item is FileInfo)
            {
                var fi = (FileInfo) item.Item;
                var supportedImage = fi.Extension.ContainsAny(supportedImageExtensions);
                if (OpenInApp)
                {
                    if (supportedImage)
                        UpdatePreviewImageData(item.Item);
                }
                else
                {
                    Process.Start(item.Path);
                }
            }
            else
            {
                UpdatePreviewImageData(item.Item);
            }
        }

        private void UpdatePreviewImageData(object item, bool sameItem = false)
        {
            if (PreviewImage.Item != null)
                if (item == PreviewImage.Item && !sameItem)
                    return;

            var f = item as FileInfo;

            if (f != null)
            {
                if (f.Extension == ".hip")
                    item = new HIPFileInfo(f.FullName);
                else if (f.Extension == ".dds")
                    item = new DDSFileInfo(f.FullName);
            }

            if (PreviewImage.Item is HIPFileInfo || PreviewImage.Item is FileInfo)
                UpdateTransparency();

            if (item is FileInfo)
            {
                var fi = (FileInfo) item;
                UpdateZoomBox(fi, PreviewImage.Item as FileSystemInfo);

                if (PreviewImage.Item != null &&
                    PreviewImage.Palette != null &&
                    (PreviewImage.PalettePath != null || TransparentBackground))
                {
                    var bitmapSource = GetSourceFromFileInfo(fi);
                    if (bitmapSource == null)
                        return;
                    PreviewImage.Source = bitmapSource.ChangePalette(PreviewImage.Palette);
                }
                else
                {
                    PreviewImage.Palette = null;
                    PreviewImage.PalettePath = null;
                    var bitmapSource = GetSourceFromFileInfo(fi);

                    if (bitmapSource == null)
                        return;
                    PreviewImage.Source = bitmapSource;
                    if (PreviewImage.Source == null)
                        return;
                    PreviewImage.HasPalette = bitmapSource.Palette != null;
                    TransparentBackground = false;
                }

                if (PreviewImage.Path == fi.FullName)
                    return;
                PreviewImage.Item = fi;
                PreviewImage.Path = fi.FullName;
                if (TransparentBackground && PreviewImage.HasPalette)
                    UpdateTransparency();
                PreviewImage.CanvasWidth = (int) PreviewImage.Source.Width;
                PreviewImage.CanvasHeight = (int) PreviewImage.Source.Height;
                PreviewImage.ImageWidth = (int) PreviewImage.Source.Width;
                PreviewImage.ImageHeight = (int) PreviewImage.Source.Height;
                PreviewImage.FileSize = (ulong) fi.Length;
            }
            else if (item is HIPFileInfo)
            {
                var hipFileInfo = (HIPFileInfo) item;
                UpdateZoomBox(hipFileInfo, PreviewImage.Item as VirtualFileSystemInfo);
                if (PreviewImage.Item is HIPFileInfo)
                {
                    var viewedHIP = (HIPFileInfo) PreviewImage.Item;
                    if (viewedHIP != null)
                    {
                        if (viewedHIP.Parent != null && hipFileInfo.Parent != null)
                        {
                            if (viewedHIP.Parent.FullName != hipFileInfo.Parent.FullName)
                            {
                                PreviewImage.PalettePath = null;
                                TransparentBackground = false;
                            }
                        }
                        else
                        {
                            if (viewedHIP.FullName != hipFileInfo.FullName)
                            {
                                PreviewImage.PalettePath = null;
                                TransparentBackground = false;
                            }
                        }
                    }
                }

                if (PreviewImage.Item != null &&
                    PreviewImage.Palette != null &&
                    (PreviewImage.PalettePath != null || TransparentBackground))
                {
                    if (string.IsNullOrWhiteSpace(PreviewImage.PalettePath))
                    {
                        if (hipFileInfo.Palette != null)
                        {
                            PreviewImage.Palette = hipFileInfo.Palette.ToBitmapPalette();
                        }
                        else
                        {
                            var colors = new Color[256];
                            colors.Populate(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
                            PreviewImage.Palette = new BitmapPalette(colors);
                        }

                        UpdateTransparency();
                    }

                    using (var bitmap = hipFileInfo.GetImage(PreviewImage.Palette.ToDrawingColors()))
                    {
                        PreviewImage.Source = bitmap.BitmapSource();
                    }
                }
                else
                {
                    PreviewImage.Palette = null;
                    PreviewImage.PalettePath = null;
                    using (var bitmap = hipFileInfo.GetImage())
                    {
                        PreviewImage.Source = bitmap.BitmapSource();
                    }

                    TransparentBackground = false;
                }

                if (hipFileInfo.NoAccess)
                {
                    PreviewImage.Path = string.Empty;
                    PreviewImage.PalettePath = string.Empty;
                    PreviewImage.PaletteItem = null;
                    PreviewImage.Item = null;
                    PreviewImage.Source = null;
                    return;
                }

                PreviewImage.Path = hipFileInfo.FullName;
                PreviewImage.CanvasWidth = hipFileInfo.CanvasWidth;
                PreviewImage.CanvasHeight = hipFileInfo.CanvasHeight;
                PreviewImage.ImageWidth = hipFileInfo.ImageWidth;
                PreviewImage.ImageHeight = hipFileInfo.ImageHeight;
                PreviewImage.OffsetX = hipFileInfo.OffsetX;
                PreviewImage.OffsetY = hipFileInfo.OffsetY;
                PreviewImage.FileSize = hipFileInfo.FileLength;
                PreviewImage.HasPalette = hipFileInfo.PixelFormat == PixelFormat.Format8bppIndexed;
                PreviewImage.MissingPalette = hipFileInfo.MissingPalette;
                PreviewImage.Item = hipFileInfo;
            }
            else if (item is DDSFileInfo)
            {
                var ddsFileInfo = (DDSFileInfo) item;
                if (PreviewImage.Path == ddsFileInfo.FullName)
                    return;
                PreviewImage.Item = ddsFileInfo;
                PreviewImage.Path = ddsFileInfo.FullName;
                PreviewImage.HasPalette = false;
                PreviewImage.PalettePath = null;
                PreviewImage.Palette = null;
                PreviewImage.Source = ddsFileInfo.GetImage().ImageSource();
                if (PreviewImage.Source == null)
                    return;
                PreviewImage.CanvasWidth = (int) PreviewImage.Source.Width;
                PreviewImage.CanvasHeight = (int) PreviewImage.Source.Height;
                PreviewImage.ImageWidth = (int) PreviewImage.Source.Width;
                PreviewImage.ImageHeight = (int) PreviewImage.Source.Height;
                PreviewImage.FileSize = ddsFileInfo.FileLength;
                UpdateZoomBox(ddsFileInfo, PreviewImage.Item as VirtualFileSystemInfo);
            }
            else
            {
                PreviewImage.Path = string.Empty;
                PreviewImage.PalettePath = string.Empty;
                PreviewImage.PaletteItem = null;
                PreviewImage.Item = null;
                PreviewImage.Source = null;
            }
        }

        private void ExploreItem(DirectoryItem item, bool loadThumbnails)
        {
            SetItemActive(item.Item);

            if (item.Items.Count > 0)
            {
                LoadingFilesTaskTokenSource = new CancellationTokenSource();

                var LoadingFilesTaskToken = LoadingFilesTaskTokenSource.Token;

                Task.Run(() =>
                {
                    if (item.Items[0] is DirectoryItemDummy)
                    {
                        var dirItemList = new List<DirectoryItem>();
                        if (item.Item is PACFileInfo)
                        {
                            dirItemList.AddRange(ExplorePACFiles(item));
                        }
                        else
                        {
                            dirItemList.AddRange(ExploreDirectories(item));
                            dirItemList.AddRange(ExploreFiles(item));
                        }

                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            item.Items.Clear();
                            item.Items.AddRange(dirItemList);
                        }));
                    }

                    if (loadThumbnails)
                    {
                        LoadingThumbsTaskTokenSource = new CancellationTokenSource();
                        var LoadingThumbsTaskToken = LoadingThumbsTaskTokenSource.Token;
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Task.Run(() => { LoadThumbnailsAsync(item, LoadingThumbsTaskToken); },
                                LoadingThumbsTaskToken);
                        }));
                    }
                }, LoadingFilesTaskToken);
            }
        }

        private void SetItemActive(object item)
        {
            var vfsi = item as VirtualFileSystemInfo;
            if (vfsi != null)
            {
                vfsi = vfsi.VirtualRoot;
                if (curVirtualFile != null && curVirtualFile != vfsi) curVirtualFile.VirtualRoot.Active = false;
                vfsi.Active = true;
                curVirtualFile = vfsi;
            }
        }

        private void CollapseItem(DirectoryItem item)
        {
            var vfsi = item.Item as VirtualFileSystemInfo;
            if (vfsi != null) vfsi.VirtualRoot.Active = false;

//            if (CurDirItem == item)
//                return;
//            item.Items.Clear();
//            if (item.Item is FileSystemInfo)
//                item.Items = SetDummy((FileSystemInfo)item.Item);
//            else if (item.Item is DriveInfo)
//                item.Items = SetDummy(((DriveInfo) item.Item).RootDirectory.FullName);
        }

        private async void LoadThumbnailsAsync(DirectoryItem ditem, CancellationToken ct)
        {
            var items = ditem.Items.ToList();
            foreach (var item in items)
            {
                if (ct.IsCancellationRequested) return;
                uiContext.Send(x => item.LoadThumbnail(), null);
                await Task.Delay(10);
            }
        }

        private void ImportPalette_Mediator(object obj)
        {
            var args = (object[]) obj;
            if (args[0] == null || args[1] == null)
                return;
            var vfsi = (VirtualFileSystemInfo) args[0];
            PreviewImage.PaletteItem = vfsi;
            PreviewImage.PalettePath = vfsi.FullName;
            PreviewImage.Palette = (BitmapPalette) args[1];
            originalBGColor = PreviewImage.Palette.Colors[0];
            UpdatePreviewImageData((FileSystemInfo) PreviewImage.Item, true);
        }

        private void UpdateContextMenu_Mediator(object obj)
        {
            var dirItem = (DirectoryItem) obj;
            dirItem.ContextMenuHasOpened = true;
            if (dirItem.Item is HIPFileInfo)
            {
                var hipFileInfo = (HIPFileInfo) dirItem.Item;
                if (hipFileInfo.Palette != null &&
                    hipFileInfo.PixelFormat == PixelFormat.Format8bppIndexed &&
                    !hipFileInfo.MissingPalette)
                    dirItem.MenuItems.AddRange(DynHIPFileMenuItems);
            }
        }

        private void UpdateZoomBox(FileSystemInfo fsi, FileSystemInfo pfsi)
        {
            if (pfsi != null)
            {
                if (Path.GetDirectoryName(fsi.FullName) != Path.GetDirectoryName(pfsi.FullName))
                    WillResetZoomBox = true;
            }
            else
            {
                WillResetZoomBox = true;
            }
        }

        private void UpdateZoomBox(VirtualFileSystemInfo vfsi, VirtualFileSystemInfo pvfsi)
        {
            if (pvfsi != null)
            {
                if (vfsi.Parent == null)
                {
                    if (Path.GetDirectoryName(vfsi.FullName) != Path.GetDirectoryName(pvfsi.FullName))
                        WillResetZoomBox = true;
                }
                else if (vfsi.Parent != pvfsi.Parent)
                {
                    WillResetZoomBox = true;
                }
            }
            else
            {
                WillResetZoomBox = true;
            }
        }

        private void UpdateTransparency()
        {
            var viewedHIP = PreviewImage.Item as HIPFileInfo;
            if (viewedHIP != null)
            {
                if (viewedHIP.Palette == null)
                    return;

                if (PreviewImage.Palette == null)
                {
                    PreviewImage.Palette = new BitmapPalette(viewedHIP.Palette.ToMediaColors());
                    originalBGColor = viewedHIP.Palette[0].ToMediaColor();
                }
            }
            else if (PreviewImage.Item is FileInfo)
            {
                var bmpsrc = PreviewImage.Source as BitmapSource;
                if (bmpsrc == null)
                    return;

                if (PreviewImage.Palette == null)
                    originalBGColor = ((BitmapSource) PreviewImage.Source).Palette.Colors[0];
            }
            else
            {
                return;
            }

            if (PreviewImage.Palette == null)
                return;

            var color = originalBGColor;
            if (TransparentBackground)
                color = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            var colors = PreviewImage.Palette.Colors.ToArray();
            colors[0] = color;
            PreviewImage.Palette = new BitmapPalette(colors);
        }

        private BitmapSource GetSourceFromFileInfo(FileInfo fi)
        {
            try
            {
                using (Stream fileStream =
                    new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BitmapDecoder bitmapDecoder = null;
                    switch (fi.Extension)
                    {
                        case ".gif":
                            bitmapDecoder = new GifBitmapDecoder(fileStream,
                                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                            break;
                        case ".jpg":
                        case ".jpeg":
                        case ".jpe":
                        case ".jfif":
                            bitmapDecoder = new BmpBitmapDecoder(fileStream,
                                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                            break;
                        case ".png":
                            bitmapDecoder = new PngBitmapDecoder(fileStream,
                                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                            break;
                        case ".tif":
                        case ".tiff":
                            bitmapDecoder = new TiffBitmapDecoder(fileStream,
                                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                            break;
                        case ".wmp":
                            bitmapDecoder = new WmpBitmapDecoder(fileStream,
                                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                            break;
                        default:
                            bitmapDecoder = new BmpBitmapDecoder(fileStream,
                                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                            break;
                    }

                    return bitmapDecoder.Frames[0];
                }
            }
            catch
            {
                return null;
            }
        }

        private void ChangeImage(bool b)
        {
            var item = PreviewImage.Item as VirtualFileSystemInfo;
            FileSystemInfo[] files = null;
            var index = -1;
            if (item != null && item.Parent != null)
            {
                var vdi = item.Parent;

                files = vdi.Files
                    .Where(vfsi => vfsi.Extension.ContainsAny(supportedImageExtensions) ||
                                   vfsi is HIPFileInfo ||
                                   vfsi is DDSFileInfo).ToArray();

                index = Array.IndexOf(files, PreviewImage.Item);
            }
            else
            {
                var vfsi = PreviewImage.Item as VirtualFileSystemInfo;
                var fi = vfsi != null ? new FileInfo(vfsi.GetPrimaryPath()) : (FileInfo) PreviewImage.Item;
                files = fi.Directory.GetFiles()
                    .Where(f => f.Extension.ContainsAny(supportedImageExtensions)).ToArray();

                index = Array.IndexOf(files.Select(f => f.Name).ToArray(), fi.Name);
            }

            if (files == null)
                return;

            if (b)
                index++;
            else
                index--;

            if (index >= files.Length)
                index = 0;
            if (index < 0)
                index = files.Length - 1;

            UpdatePreviewImageData(files[index]);
        }

        private void ChangePalette(byte b)
        {
            var bb = !((b & 1) == 0);
            var bs = !((b & 2) == 0);

            var item = PreviewImage.PaletteItem as VirtualFileSystemInfo;
            if (item == null)
                return;

            var vdi = item.Parent;
            if (vdi == null)
                return;

            var index = Array.IndexOf(vdi.Files, item);
            if (index == -1)
                return;

            if (bb)
                index += bs ? 8 : 1;
            else
                index -= bs ? 8 : 1;

            if (index >= vdi.Files.Length)
                index = index % vdi.Files.Length;
            if (index < 0)
                index = vdi.Files.Length + index;

            var colorRange = ((BitmapSource) PreviewImage.Source).Palette.Colors.Count;
            BitmapPalette palette = null;
            var virtualFile = vdi.Files[index];
            byte[] bytes = null;
            bytes = virtualFile.GetBytes();

            if (bytes == null)
                return;
            switch (vdi.Files[index].Extension.ToLower())
            {
                case ".hpl":
                    palette = ((HPLFileInfo) virtualFile).Palette.ToBitmapPalette();
                    break;
                case ".hip":
                    palette = ((HIPFileInfo) virtualFile).Palette.ToBitmapPalette();
                    break;
                case ".act":
                    palette = PaletteTools.ReadACTPalette(bytes, colorRange);
                    break;
                case ".pal":
                    palette = PaletteTools.ReadPALPalette(bytes, colorRange);
                    break;
                case ".aco":
                    palette = PaletteTools.ReadACOPalette(bytes, colorRange);
                    break;
                case ".ase":
                    palette = PaletteTools.ReadASEPalette(bytes, colorRange);
                    break;
            }

            PreviewImage.PaletteItem = virtualFile;
            PreviewImage.Palette = palette;
            originalBGColor = PreviewImage.Palette.Colors[0];
            PreviewImage.PalettePath = virtualFile.FullName;

            UpdatePreviewImageData((FileSystemInfo) PreviewImage.Item, true);
        }

        private void UpdateSettings_Mediator(object args)
        {
            var settings = (object[]) args;
            PreemptFileAnalysis = (bool) settings[3];
            OpenInApp = (bool) settings[4];
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}