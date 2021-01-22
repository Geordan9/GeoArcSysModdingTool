using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    internal class PaletteEditorViewModel : INotifyPropertyChanged
    {
        private ObservableRangeCollection<DirectoryItem> rootDirItems = new ObservableRangeCollection<DirectoryItem>();

        private ObservableRangeCollection<PreviewImage> previewImages = new ObservableRangeCollection<PreviewImage>();

        private BitmapPalette palette;

        private bool isImageOutlined;

        private Brush backgroundColor = Brushes.Transparent;

        private bool transparentBackground;

        private Color changeColor = Colors.Transparent;

        public PaletteEditorViewModel()
        {
            Mediator.Register("UpdatePaletteEditorDirItems", obj =>
            {
                var args = (object[]) obj;
                var item = (DirectoryItem) args[0];
                var removeItem = (bool) args[1];
                UpdateDirItems(item, removeItem);
            });

            TransparentBackgroundCommand = new Command(ToggleTransparentBackground);

            ExpandCollapseCommand = new Command<object[]>(obj =>
            {
                if ((bool) obj[0])
                    ExploreItem((DirectoryItem) obj[1]);
            });

            RemoveImageCommand = new Command<PreviewImage>(RemoveImage);

            ToggleDirectoryItemCommand = new Command<DirectoryItem>(ToggleDirectoryItem);

            OpenDirectoryItemCommand = new Command(OpenDirectoryItem);

            RemoveDirectoryItemCommand = new Command<object>(obj =>
            {
                var di = obj as DirectoryItem;
                if (di != null)
                    UpdateDirItems(di, true);
            });

            MovePreviewImageForwardCommand = new Command<PreviewImage>(pi => ChangePreviewImageIndex(pi, 1));

            MovePreviewImageBackwardCommand = new Command<PreviewImage>(pi => ChangePreviewImageIndex(pi, -1));

            BringPreviewImageFrontCommand = new Command<PreviewImage>(pi => ChangePreviewImageIndex(pi, 1, true));

            BringPreviewImageBackCommand = new Command<PreviewImage>(pi => ChangePreviewImageIndex(pi, -1, true));
        }

        // properties

        public static ICommand TransparentBackgroundCommand { get; set; }

        public static ICommand ExpandCollapseCommand { get; set; }

        public static ICommand RemoveImageCommand { get; set; }

        public static ICommand ToggleDirectoryItemCommand { get; set; }

        public static ICommand OpenDirectoryItemCommand { get; set; }

        public static ICommand RemoveDirectoryItemCommand { get; set; }

        public static ICommand MovePreviewImageForwardCommand { get; set; }

        public static ICommand MovePreviewImageBackwardCommand { get; set; }

        public static ICommand BringPreviewImageFrontCommand { get; set; }

        public static ICommand BringPreviewImageBackCommand { get; set; }

        public ObservableRangeCollection<DirectoryItem> RootDirItems
        {
            get => rootDirItems;
            set
            {
                rootDirItems = value;
                OnPropertyChanged();
            }
        }

        public ObservableRangeCollection<PreviewImage> PreviewImages
        {
            get => previewImages;
            set
            {
                previewImages = value;
                OnPropertyChanged();
            }
        }

        public BitmapPalette Palette
        {
            get => palette;
            set
            {
                palette = value;
                OnPropertyChanged();
            }
        }

        public bool IsImageOutlined
        {
            get => isImageOutlined;
            set
            {
                isImageOutlined = value;
                OnPropertyChanged();
            }
        }

        public Brush BackgroundColor
        {
            get => backgroundColor;
            set
            {
                backgroundColor = value;
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

        public Color ChangeColor
        {
            get => changeColor;
            set
            {
                changeColor = value;
                OnPropertyChanged();
            }
        }

        // methods

        private void UpdateDirItems(DirectoryItem item, bool removeItem)
        {
            if (removeItem)
            {
                RootDirItems.Remove(item);
                return;
            }

            RootDirItems.Add(item);
            var vfsi = item.Item as VirtualFileSystemInfo;
            if (vfsi == null)
                return;
            vfsi.Active = true;
        }

        private void RemoveImage(PreviewImage pi)
        {
            PreviewImages.Remove(pi);
            UpdatePreviewImageIndexes();
            var dirItem = pi.Item as DirectoryItem;
            if (dirItem == null)
                return;
            dirItem.IsChecked = false;
        }

        private void ToggleDirectoryItem(DirectoryItem di)
        {
            if (!di.IsChecked)
            {
                RemoveImage(PreviewImages.First(pi => pi.Item == di));
                UpdatePreviewImageIndexes();
                return;
            }

            var hipFileInfo = di.Item as HIPFileInfo;
            if (hipFileInfo != null && di.IsChecked) PreviewImages.Add(CreatePreviewImage(di));
        }

        private void OpenDirectoryItem()
        {
            ChangeWindowOpacity(0.6);
            try
            {
                var path = Dialogs.OpenFileDialog("Open...",
                    "HIP Files|*.hip|PAC Files|*.pac;*.paccs;*.pacgz");
                if (string.IsNullOrWhiteSpace(path))
                    return;

                var ext = Path.GetExtension(path).ToLower();

                VirtualFileSystemInfo vfsi = null;
                switch (ext)
                {
                    case ".hip":
                        vfsi = new HIPFileInfo(path);
                        break;
                    case string str when str.Contains("pac"):
                        vfsi = new PACFileInfo(path);
                        break;
                }

                if (vfsi == null)
                    return;

                UpdateDirItems(GetItem(vfsi), false);
            }
            finally
            {
                ChangeWindowOpacity(1.0);
            }
        }

        private PreviewImage CreatePreviewImage(DirectoryItem item)
        {
            var hipFileInfo = (HIPFileInfo) item.Item;

            var previewImage = new PreviewImage
            {
                Source = hipFileInfo.GetImage(Palette == null ? null : Palette.ToDrawingColors()).ImageSource(),
                Palette = Palette != null ? Palette : hipFileInfo.Palette.ToBitmapPalette(),
                Path = hipFileInfo.FullName,
                CanvasWidth = hipFileInfo.CanvasWidth,
                CanvasHeight = hipFileInfo.CanvasHeight,
                ImageWidth = hipFileInfo.ImageWidth,
                ImageHeight = hipFileInfo.ImageHeight,
                OffsetX = hipFileInfo.OffsetX,
                OffsetY = hipFileInfo.OffsetY,
                FileSize = hipFileInfo.FileLength,
                HasPalette = hipFileInfo.PixelFormat == PixelFormat.Format8bppIndexed,
                MissingPalette = hipFileInfo.MissingPalette,
                Item = item,
                Zindex = PreviewImages.Count()
            };

            if (TransparentBackground) SetTransparentBackground(previewImage);

            return previewImage;
        }

        private void ExploreItem(DirectoryItem item)
        {
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
                }, LoadingFilesTaskToken);
            }
        }

        private void ToggleTransparentBackground()
        {
            foreach (var previewImage in PreviewImages)
                if (TransparentBackground)
                {
                    SetTransparentBackground(previewImage);
                }
                else
                {
                    var pixels = new byte[previewImage.ImageWidth * previewImage.ImageHeight];
                    (previewImage.Source as BitmapSource).CopyPixels(pixels, previewImage.ImageWidth, 0);
                    previewImage.Source = BitmapSource.Create(
                        previewImage.ImageWidth,
                        previewImage.ImageHeight,
                        96,
                        96,
                        PixelFormats.Indexed8,
                        previewImage.Palette,
                        pixels,
                        previewImage.ImageWidth);
                }
        }

        private void SetTransparentBackground(PreviewImage previewImage)
        {
            var pixels = new byte[previewImage.ImageWidth * previewImage.ImageHeight];
            (previewImage.Source as BitmapSource).CopyPixels(pixels, previewImage.ImageWidth, 0);
            var colors = new Color[previewImage.Palette.Colors.Count];
            colors[0] = Colors.Transparent;
            for (var i = 1; i < previewImage.Palette.Colors.Count; i++) colors[i] = previewImage.Palette.Colors[i];
            previewImage.Source = BitmapSource.Create(
                previewImage.ImageWidth,
                previewImage.ImageHeight,
                96,
                96,
                PixelFormats.Indexed8,
                new BitmapPalette(colors),
                pixels,
                previewImage.ImageWidth);
        }

        private void ChangePreviewImageIndex(PreviewImage previewImage, int pos, bool BringToEnd = false)
        {
            pos = BringToEnd ? pos > 0 ? PreviewImages.Count - 1 : 0 : pos + previewImage.Zindex;
            if (pos >= 0 && pos < PreviewImages.Count)
            {
                PreviewImages.Move(previewImage.Zindex, pos);
                UpdatePreviewImageIndexes();
            }
        }

        private void UpdatePreviewImageIndexes()
        {
            for (var i = 0; i < PreviewImages.Count; i++) PreviewImages[i].Zindex = i;
        }

        private void ChangeWindowOpacity(double opacity)
        {
            Mediator.NotifyColleagues("ChangeWindowOpacity", opacity);
        }

        // INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}