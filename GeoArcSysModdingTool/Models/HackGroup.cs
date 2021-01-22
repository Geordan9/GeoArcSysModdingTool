using System.ComponentModel;
using System.Runtime.CompilerServices;
using GHLib.Models;
using static GHLib.Utils.HackTools;

namespace GeoArcSysModdingTool.Models
{
    public class HackGroup : INotifyPropertyChanged
    {
        public string Name { get; set; }

        private Hack[] hacks;

        public Hack[] Hacks
        {
            get => hacks;
            set
            {
                if (hacks != null)
                    foreach (var h in GetAllHacks(hacks))
                        h.PropertyChanged -= HackPropertyChanged;

                hacks = value;

                if (hacks != null)
                    foreach (var h in GetAllHacks(hacks))
                        h.PropertyChanged += HackPropertyChanged;
            }
        }

        private void HackPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged();
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