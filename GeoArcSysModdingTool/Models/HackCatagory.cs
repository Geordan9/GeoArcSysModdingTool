using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using static GHLib.Utils.HackTools;

namespace GeoArcSysModdingTool.Models
{
    public class HackCatagory : INotifyPropertyChanged
    {
        public string Name { get; set; }

        private bool visible;

        public bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                OnPropertyChanged();
            }
        }

        private readonly ProgressInfo progressInfo = new ProgressInfo();

        public ProgressInfo ProgressInfo
        {
            get
            {
                var booleans = GetAllHacks(HackGroups.SelectMany(hg => hg.Hacks).ToArray())
                    .Where(h => h.AoBScripts != null && h.AoBScripts.Length != 0)
                    .Select(h => h.Initialized)
                    .ToArray();
                var enabled = !booleans.All(b => b) && !booleans.All(b => !b);

                progressInfo.IsEnabled = progressInfo.Visible = enabled;
                progressInfo.Maximum = booleans.Length;
                progressInfo.Value = booleans.Where(b => b).Count();
                return progressInfo;
            }
        }

        private HackGroup[] hackGroups;

        public HackGroup[] HackGroups
        {
            get => hackGroups;
            set
            {
                if (hackGroups != null)
                    foreach (var hg in hackGroups)
                        hg.PropertyChanged -= HackPropertyChanged;

                hackGroups = value;

                if (hackGroups != null)
                    foreach (var hg in hackGroups)
                        hg.PropertyChanged += HackPropertyChanged;
            }
        }

        private void HackPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("ProgressInfo");
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