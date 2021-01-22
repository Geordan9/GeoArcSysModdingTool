using System.Drawing;
using System.Windows.Media.Imaging;

namespace GeoArcSysModdingTool.Utils.Extensions
{
    public static class BitmapPaletteExtension
    {
        public static Color[] ToDrawingColors(this BitmapPalette bp)
        {
            var dcolors = new Color[bp.Colors.Count];
            for (var i = 0; i < bp.Colors.Count; i++)
                dcolors[i] =
                    Color.FromArgb(bp.Colors[i].A, bp.Colors[i].R, bp.Colors[i].G, bp.Colors[i].B);
            return dcolors;
        }
    }
}