using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GeoArcSysModdingTool.Models
{
    public class PreviewImage : INotifyPropertyChanged
    {
        private ImageSource source;
        private string path;
        private int canvasWidth;
        private int canvasHeight;
        private int imageWidth;
        private int imageHeight;
        private int offsetX;
        private int offsetY;
        private ulong fileSize;
        private bool hasPalette;
        private bool missingPalette;
        private BitmapPalette palette;
        private string palettePath;
        private object item;
        private object paletteItem;
        private bool flippedX;
        private bool flippedY;
        private int zindex;

        public ImageSource Source
        {
            get => source;
            set
            {
                source = value;
                OnPropertyChanged();
            }
        }

        public string Path
        {
            get => path;
            set
            {
                path = value;
                OnPropertyChanged();
            }
        }

        public int CanvasWidth
        {
            get => canvasWidth;
            set
            {
                canvasWidth = value;
                OnPropertyChanged();
            }
        }

        public int CanvasHeight
        {
            get => canvasHeight;
            set
            {
                canvasHeight = value;
                OnPropertyChanged();
            }
        }

        public int ImageWidth
        {
            get => imageWidth;
            set
            {
                imageWidth = value;
                OnPropertyChanged();
            }
        }

        public int ImageHeight
        {
            get => imageHeight;
            set
            {
                imageHeight = value;
                OnPropertyChanged();
            }
        }

        public int OffsetX
        {
            get => offsetX;
            set
            {
                offsetX = value;
                OnPropertyChanged();
            }
        }

        public int OffsetY
        {
            get => offsetY;
            set
            {
                offsetY = value;
                OnPropertyChanged();
            }
        }

        public ulong FileSize
        {
            get => fileSize;
            set
            {
                fileSize = value;
                OnPropertyChanged();
            }
        }

        public bool HasPalette
        {
            get => hasPalette;
            set
            {
                hasPalette = value;
                OnPropertyChanged();
            }
        }

        public bool MissingPalette
        {
            get => missingPalette;
            set
            {
                missingPalette = value;
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

        public string PalettePath
        {
            get => palettePath;
            set
            {
                palettePath = value;
                OnPropertyChanged();
            }
        }

        public object Item
        {
            get => item;
            set
            {
                item = value;
                OnPropertyChanged();
            }
        }

        public object PaletteItem
        {
            get => paletteItem;
            set
            {
                paletteItem = value;
                OnPropertyChanged();
            }
        }

        public bool FlippedX
        {
            get => flippedX;
            set
            {
                flippedX = value;
                OnPropertyChanged();
            }
        }

        public bool FlippedY
        {
            get => flippedY;
            set
            {
                flippedY = value;
                OnPropertyChanged();
            }
        }

        public int Zindex
        {
            get => zindex;
            set
            {
                zindex = value;
                OnPropertyChanged();
            }
        }

        // INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}