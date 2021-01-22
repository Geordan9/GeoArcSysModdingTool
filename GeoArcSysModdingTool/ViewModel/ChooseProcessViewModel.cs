using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using GeoArcSysModdingTool.Components;
using ProcessLib.Models;
using ProcessLib.Utils;
using ProcessLib.Utils.Extensions;

namespace GeoArcSysModdingTool.ViewModel
{
    public class ChooseProcessViewModel : INotifyPropertyChanged
    {
        private static readonly string[] ignoreProcessList =
        {
            "svchost",
            "conhost",
            "idle",
            "services",
            "csrss",
            "wininit",
            "smss",
            "System",
            "Registry",
            "dwm",
            "fontdrvhost",
            "winlogon",
            "lsass",
            "Memory Compression",
            "SgrmBroker",
            "Idle"
        };

        private static readonly Process currentProcess = Process.GetCurrentProcess();

        private bool _isButtonLogicComplete;

        private bool _isRefreshing;

        public ObservableCollection<ProcessSnapshot> _ProcessSnaphots = new ObservableCollection<ProcessSnapshot>();

        public ChooseProcessViewModel()
        {
            // initialize commands
            SelectProcessCommand = new Command<int>(SelectProcess);
            RefreshProcessesCommand = new Command(GetProcesses);
            GetProcesses();
        }

        // properties


        public ObservableCollection<ProcessSnapshot> ProcessSnaphots
        {
            get => _ProcessSnaphots;
            set
            {
                _ProcessSnaphots = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectProcessCommand { get; set; }

        public ICommand RefreshProcessesCommand { get; set; }

        public int ProcessID { get; set; }

        public bool isButtonLogicComplete
        {
            get => _isButtonLogicComplete;
            set
            {
                _isButtonLogicComplete = value;
                OnPropertyChanged();
            }
        }

        public bool isRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        // INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        // methods

        private async void GetProcesses()
        {
            isRefreshing = true;
            var tempList = new List<ProcessSnapshot>();
            await Task.Run(() =>
            {
                var processes = Process.GetProcesses().Where(prc =>
                {
                    var name = prc.ProcessName;
                    if (ignoreProcessList.Contains(name) || string.IsNullOrWhiteSpace(name) ||
                        prc.Id == currentProcess.Id)
                        return false;
                    if (Check64bit.is64bitOS)
                        if (!Check64bit.is64bitProcess)
                            try
                            {
                                if (!prc.IsWin64Emulator())
                                    return false;
                            }
                            catch
                            {
                                return false;
                            }

                    try
                    {
                        var starttime = prc.StartTime;
                        return true;
                    }
                    catch
                    {
                    }

                    return false;
                }).OrderByDescending(p => p.StartTime).ToArray();
                foreach (var process in processes) tempList.Add(new ProcessSnapshot(process));
            });
            ProcessSnaphots.Clear();
            Array.ForEach(tempList.ToArray(), prcsnap => { ProcessSnaphots.Add(prcsnap); });
            isRefreshing = false;
        }

        private void SelectProcess(int ID)
        {
            ProcessID = ID;
            isButtonLogicComplete = true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}