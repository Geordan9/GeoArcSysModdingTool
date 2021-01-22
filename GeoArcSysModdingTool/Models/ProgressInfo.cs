using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GeoArcSysModdingTool.Models
{
    public class ProgressInfo : INotifyPropertyChanged
    {
        private int _Maximum;
        private int _Value;
        private bool _Visible;
        private bool isEnabled;

        public int Value
        {
            get => _Value;
            set
            {
                _Value = value;
                OnPropertyChanged();
            }
        }

        public int Maximum
        {
            get => _Maximum;
            set
            {
                _Maximum = value;
                OnPropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool Visible
        {
            get => _Visible;
            set
            {
                _Visible = value;
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