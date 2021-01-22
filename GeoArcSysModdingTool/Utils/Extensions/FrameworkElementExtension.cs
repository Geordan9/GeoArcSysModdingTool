using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GeoArcSysModdingTool.Utils.Extensions
{
    public static class FrameworkElementExtension
    {
        public static RenderTargetBitmap GetRenderImage(this FrameworkElement view)
        {
            var size = new Size(view.ActualWidth, view.ActualHeight);
            if (size.IsEmpty)
                return null;

            var result = new RenderTargetBitmap((int) size.Width, (int) size.Height, 96, 96, PixelFormats.Pbgra32);

            var drawingvisual = new DrawingVisual();
            using (var context = drawingvisual.RenderOpen())
            {
                context.DrawRectangle(new VisualBrush(view), null, new Rect(new Point(), size));
                context.Close();
            }

            result.Render(drawingvisual);
            return result;
        }
    }
}