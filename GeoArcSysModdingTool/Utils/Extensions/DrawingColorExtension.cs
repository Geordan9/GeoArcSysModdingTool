using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GeoArcSysModdingTool.Utils.Extensions
{
    public static class DrawingColorExtension
    {
        public static Color ToMediaColor(this System.Drawing.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Color[] ToMediaColors(this System.Drawing.Color[] colors)
        {
            var mcolors = new Color[colors.Length];
            for (var i = 0; i < colors.Length; i++)
                mcolors[i] = Color.FromArgb(colors[i].A, colors[i].R, colors[i].G, colors[i].B);
            return mcolors;
        }

        public static BitmapPalette ToBitmapPalette(this System.Drawing.Color[] colors)
        {
            return new BitmapPalette(colors.ToMediaColors());
        }
    }
}