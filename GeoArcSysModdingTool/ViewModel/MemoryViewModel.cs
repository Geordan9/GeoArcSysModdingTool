using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GeoArcSysModdingTool.Components;
using GeoArcSysModdingTool.Models;
using GeoArcSysModdingTool.Properties;
using GeoArcSysModdingTool.Utils;
using GHLib.Models;
using MemoryLib;
using ProcessLib.Utils.Extensions;
using static GHLib.Globals;
using static GHLib.Utils.HackTools;

namespace GeoArcSysModdingTool.ViewModel
{
    public class MemoryViewModel : INotifyPropertyChanged
    {
        private static Process myProcess;

        private static string processFilePath;

        private bool isProcessOpen;

        private bool isProcessSuspended;

        private TimeSpan processUpdateInterval = TimeSpan.FromMilliseconds(750);

        private ObservableCollection<bool> visibleTabs = new ObservableCollection<bool>
        {
            true
        };

        private bool isRestarting;

        private int processId;

        private CancellationTokenSource updateProcessTaskTokenSource = new CancellationTokenSource();

        private static readonly string hackModuleLocation = Path.Combine(Globals.CurrentDirectory, "ArcSys.ghm");

        private HackCatagory[] hackCatagories = File.Exists(hackModuleLocation)
            ? GHBinaryTools.ReadBinaryHackModule(hackModuleLocation)
            : GHBinaryTools.ReadBinaryHackModule(new MemoryStream(Resources.ArcSys));

        public MemoryViewModel()
        {
            HackToolSettings.AutoUpdateHacks = true;
            HackToolSettings.ReverseScan = true;
            Hacks = HackCatagories.SelectMany(hc => hc.HackGroups.SelectMany(hg => hg.Hacks)).ToArray();

            Mediator.Register("SelectProcess", SelectProcess_Mediator);
            Mediator.Register("UpdateSettings", UpdateSettings_Mediator);

            ToggleHackCommand = new Command<Hack>(h => ToggleHack(h));

            KillProcessCommand = new Command(KillProcess);

            RestartProcessCommand = new Command(RestartProcess);

            SuspendResumeProcessCommand = new Command(SuspendResumeProcess);

            HackInputTextChangeCommand = new Command<object[]>(obj => HackInputTextChanged(obj));
        }

        // properties

        public ICommand ToggleHackCommand { get; set; }

        public ICommand KillProcessCommand { get; set; }

        public ICommand RestartProcessCommand { get; set; }

        public ICommand SuspendResumeProcessCommand { get; set; }

        public ICommand HackInputTextChangeCommand { get; set; }

        public Process MyProcess
        {
            get => myProcess;
            set
            {
                myProcess = value;
                OnPropertyChanged();
            }
        }

        public bool IsProcessOpen
        {
            get => isProcessOpen;
            set
            {
                isProcessOpen = value;
                OnPropertyChanged();
            }
        }

        public bool IsProcessSuspended
        {
            get => isProcessSuspended;
            set
            {
                isProcessSuspended = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan ProcessUpdateInterval
        {
            get => processUpdateInterval;
            set
            {
                processUpdateInterval = value;
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

        public HackCatagory[] HackCatagories
        {
            get => hackCatagories;
            set
            {
                hackCatagories = value;
                OnPropertyChanged();
            }
        }

        // methods

        private void SelectProcess_Mediator(object args)
        {
            var process = args as Process;

            if (process != null && process.Id != processId)
                try
                {
                    process.EnableRaisingEvents = true;
                    process.Exited += Process_Exited;
                    GHMemory.ReadProcess = process;
                    GHSigScan = new SigScan(process, GHMemory.ReadProcess.MainModule.BaseAddress,
                        GHMemory.ReadProcess.MainModule.ModuleMemorySize);

                    UpdateProcess(process);

                    IsProcessSuspended = MyProcess.IsSuspended();
                    IsProcessOpen = true;
                }
                catch
                {
                }
        }

        private void UpdateProcess(Process process)
        {
            MyProcess = process;

            processId = process.Id;

            updateProcessTaskTokenSource = new CancellationTokenSource();

            var updateProcessTaskToken = updateProcessTaskTokenSource.Token;

            Task.Run(async () =>
            {
                while (!updateProcessTaskToken.IsCancellationRequested)
                {
                    await Task.Delay(ProcessUpdateInterval);
                    try
                    {
                        MyProcess = Process.GetProcessById(MyProcess.Id);

                        if (MyProcess.HasExited)
                        {
                            updateProcessTaskTokenSource.Cancel();
                            break;
                        }

                        IsProcessSuspended = MyProcess.IsSuspended();
                    }
                    catch
                    {
                        IsProcessOpen = false;
                        IsProcessSuspended = false;
                        Reset();
                        updateProcessTaskTokenSource.Cancel();
                        break;
                    }
                }
            }, updateProcessTaskToken);
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            if (MyProcess != null)
            {
                updateProcessTaskTokenSource.Cancel();
                var info = MyProcess.StartInfo;
                if (processFilePath != null)
                {
                    info.FileName = processFilePath;
                    info.WorkingDirectory = Path.GetDirectoryName(info.FileName);
                    processFilePath = null;
                }

                MyProcess.Refresh();
                IsProcessOpen = false;
                IsProcessSuspended = false;
                MyProcess = null;

                if (isRestarting && !string.IsNullOrWhiteSpace(info.FileName))
                    SelectProcess_Mediator(Process.Start(info));
            }
        }

        private void KillProcess()
        {
            Task.Run(() =>
            {
                if (MyProcess != null) MyProcess.Kill();
            });
        }

        private void RestartProcess()
        {
            isRestarting = true;
            processFilePath = MyProcess.MainModule.FileName;
            KillProcess();
        }

        private void SuspendResumeProcess()
        {
            if (!IsProcessSuspended)
                MyProcess.Suspend();
            else
                MyProcess.Resume();
            IsProcessSuspended = !IsProcessSuspended;
        }

        private void UpdateSettings_Mediator(object args)
        {
            var settings = (object[]) args;
            HackToolSettings.ReverseScan = (bool) settings[0];
            HackToolSettings.ValueUpdateInterval = TimeSpan.FromMilliseconds((uint) settings[1]);
            ProcessUpdateInterval = TimeSpan.FromMilliseconds((uint) settings[2]);
        }

        private void HackInputTextChanged(object[] parameters)
        {
            var userInput = (bool) parameters[0];
            var hackInput = (HackInput) parameters[1];
            hackInput.Editing = userInput;
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