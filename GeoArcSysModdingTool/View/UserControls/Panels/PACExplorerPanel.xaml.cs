using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ArcSysAPI.Models;
using GeoArcSysModdingTool.Models;
using GeoArcSysModdingTool.Utils;
using GeoArcSysModdingTool.Utils.Extensions;
using GeoArcSysModdingTool.View.CustomControls;
using GeoArcSysModdingTool.View.UserControls.Dialogs;

namespace GeoArcSysModdingTool.View.UserControls.Panels
{
    public partial class PACExplorerPanel : UserControl
    {
        public PACExplorerPanel()
        {
            InitializeComponent();
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                new Action(() => { Mediator.NotifyColleagues("LoadDirectories", null); }));
        }

        private void SavePreviewImage_Click(object sender, RoutedEventArgs e)
        {
            var mnu = sender as MenuItem;
            if (mnu != null)
            {
                ChangeWindowOpacity(0.6);
                var img = ((ContextMenu) mnu.Parent).PlacementTarget as Image;
                if (img.Source != null)
                    ((BitmapSource) img.Source).SaveImageAs(Path.GetFileNameWithoutExtension((string) img.Tag));
                ChangeWindowOpacity(1.0);
            }
        }

        private void SavePreviewImagePalette_Click(object sender, RoutedEventArgs e)
        {
            var mnu = sender as MenuItem;
            if (mnu != null)
            {
                ChangeWindowOpacity(0.6);
                var img = ((ContextMenu) mnu.Parent).PlacementTarget as Image;
                if (img.Source != null)
                    SavePreviewImagePalette((BitmapSource) img.Source,
                        Path.GetFileNameWithoutExtension((string) img.Tag));
                ChangeWindowOpacity(1.0);
            }
        }

        private void SavePreviewImagePalette(BitmapSource bmpsrc, string fileName)
        {
            var path = Utils.Dialogs.SaveFileDialog("Save Palette As...",
                "HPL File|*.hpl|ACT File|*.act",
                fileName);
            if (string.IsNullOrWhiteSpace(path))
                return;

            var colors = bmpsrc.Palette.Colors.ToArray();
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

            try
            {
                if (fileBytes.Length > 0)
                    File.WriteAllBytes(path, fileBytes);
            }
            catch
            {
            }
        }

        private void ImportPalButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeWindowOpacity(0.6);
            try
            {
                var colorRange = ((BitmapSource) PreviewImage.Source).Palette.Colors.Count;
                var path = Utils.Dialogs.OpenFileDialog("Open...",
                    "Palette Files|*.act;*.pal|Swatches|*.aco;*.ase|ArcSys Palettes|*.hpl;*pal.pac|ArcSys Images|*.hip;*img.pac;*vri.pac|ArcSys Files|*.pac;*.paccs;*.pacgz");
                if (string.IsNullOrWhiteSpace(path))
                    return;
                var ext = Path.GetExtension(path).ToLower();
                byte[] bytes = null;
                BitmapPalette palette = null;
                VirtualFileSystemInfo virtualFile = null;
                if (ext == ".pac" || ext == ".pacgz")
                {
                    var pacItemSelectorDialog = new PACItemSelectorDialog(new PACFileInfo(path));
                    pacItemSelectorDialog.Owner = Window.GetWindow(this);

                    if (pacItemSelectorDialog.ShowDialog() != true)
                        return;

                    virtualFile = pacItemSelectorDialog.SelectedItem;
                    ext = virtualFile.Extension;
                    bytes = virtualFile.GetBytes();
                }
                else if (ext == ".hpl")
                {
                    virtualFile = new HPLFileInfo(path);
                    bytes = virtualFile.GetBytes();
                }
                else if (ext == ".hip")
                {
                    virtualFile = new HIPFileInfo(path);
                    if (((HIPFileInfo) virtualFile).Palette == null)
                        return;
                    bytes = virtualFile.GetBytes();
                }
                else
                {
                    bytes = File.ReadAllBytes(path);
                }

                if (bytes == null)
                    return;
                switch (ext)
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

                //var palDialog = new PACItemSelectorDialog();
                Mediator.NotifyColleagues("ImportPalette", new object[]
                {
                    virtualFile,
                    palette
                });
            }
            finally
            {
                ChangeWindowOpacity(1.0);
            }
        }

        private void ChangeWindowOpacity(double opacity)
        {
            Mediator.NotifyColleagues("ChangeWindowOpacity", opacity);
        }

        private void ZoomBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var zb = (ZoomBorder) sender;
            Keyboard.Focus(zb);
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed
                || e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                zb.Reset();
        }

        private void DirectoryItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var dirItem = (DirectoryItem) ((FrameworkElement) sender).DataContext;
            if (!dirItem.ContextMenuHasOpened)
                Mediator.NotifyColleagues("UpdateContextMenu", dirItem);
        }

        private void ImageGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue) Keyboard.Focus(ZoomBorder);
        }

        private void ZoomBorder_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Left || e.Key == Key.Right) &&
                (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
                // Move your camera here
                e.Handled = true;
        }
    }
}