using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GeoArcSysModdingTool.Components;
using GeoArcSysModdingTool.Utils;
using static GHLib.Utils.HackTools;

namespace GeoArcSysModdingTool.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool isDrawerOpen;

        private ObservableCollection<bool> visibleTabs = new ObservableCollection<bool>
        {
            true,
            false,
            false,
            false,
            false
        };

        private double windowOpacity = 1.0;

        public MainViewModel()
        {
            Mediator.Register("ChangeWindowOpacity", ChangeWindowOpacity_Mediator);
            Mediator.Register("UpdateSettings", UpdateSettings_Mediator);

            DeactivateAllHacksCommand = new Command(() => DisableHacks(true));
        }

        // properties

        public ICommand DeactivateAllHacksCommand { get; set; }

        public double WindowOpacity
        {
            get => windowOpacity;
            set
            {
                windowOpacity = value;
                OnPropertyChanged();
            }
        }

        public bool IsDrawerOpen
        {
            get => isDrawerOpen;
            set
            {
                isDrawerOpen = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<bool> VisibleTabs
        {
            get => visibleTabs;
            set
            {
                visibleTabs = value;
                OnPropertyChanged();
            }
        }

        // methods

        private void ChangeWindowOpacity_Mediator(object args)
        {
            WindowOpacity = (double) args;
        }

        private void UpdateSettings_Mediator(object args)
        {
            var settings = (object[]) args;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}