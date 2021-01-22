using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GeoArcSysModdingTool.Models;
using GeoArcSysModdingTool.Utils.Extensions;
using GeoArcSysModdingTool.View.CustomControls;

namespace GeoArcSysModdingTool.View.UserControls.Panels
{
    /// <summary>
    ///     Interaction logic for PaletteEditorPanel.xaml
    /// </summary>
    public partial class PaletteEditorPanel : UserControl
    {
        public PaletteEditorPanel()
        {
            InitializeComponent();
        }

        private void ZoomBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var zb = (ZoomBorder) sender;
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed
                || e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                zb.Reset();
        }

        private void ContentPresenter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((ContentPresenter) sender).Focus();
        }

        private void SaveViewButton_Click(object sender, RoutedEventArgs e)
        {
            ImageGridView.GetRenderImage().SaveImageAs();
        }

        private void FlipHorizontalMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var pi = (PreviewImage) ((MenuItem) sender).DataContext;
            pi.FlippedX = !pi.FlippedX;
        }

        private void FlipVerticalMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var pi = (PreviewImage) ((MenuItem) sender).DataContext;
            pi.FlippedY = !pi.FlippedY;
        }

        private void SavePreviewImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ((BitmapSource) ((PreviewImage) ((MenuItem) sender).DataContext).Source).SaveImageAs();
        }
    }
}