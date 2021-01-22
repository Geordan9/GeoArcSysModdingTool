using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ArcSysAPI.Models;
using GeoArcSysModdingTool.Utils;

namespace GeoArcSysModdingTool.Models
{
    public class DirectoryItem : INotifyPropertyChanged
    {
        private ObservableRangeCollection<DirectoryItem> items = new ObservableRangeCollection<DirectoryItem>();

        private string name;
        private bool showThumbnail;
        private bool isExpanded;
        private bool isChecked;
        private bool isSelected;
        private Visibility visibility;

        public DirectoryItem(object item = null, int shellIconIndex = 0)
        {
            Item = item;
            ShellIconIndex = shellIconIndex;
        }

        // Properties

        public string Name
        {
            get
            {
                if (name == null)
                    return System.IO.Path.GetFileName(Path);
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string path;

        public string Path
        {
            get => path;
            set
            {
                path = value;
                OnPropertyChanged();
            }
        }

        public int ShellIconIndex { get; set; }

        public bool ShowThumbnail
        {
            get => showThumbnail;
            private set
            {
                showThumbnail = value;
                OnPropertyChanged();
            }
        }

        public object Item { get; set; }

        public List<DirectoryItemMenuItem> MenuItems { get; set; } = new List<DirectoryItemMenuItem>();

        public ObservableRangeCollection<DirectoryItem> Items
        {
            get => items;
            set
            {
                if (items != value)
                    items = value;
                OnPropertyChanged();
            }
        }

        private FileSystemWatcher fsw;

        public FileSystemWatcher FSW
        {
            get => fsw;
            set
            {
                fsw = value;
                fsw.Changed += OnChanged;
                fsw.Created += OnChanged;
                fsw.Deleted += OnChanged;
                fsw.Renamed += OnRenamed;
            }
        }

        public bool ContextMenuHasOpened { get; set; } = false;

        public Brush? TextColor
        {
            get
            {
                if (Item is VirtualFileSystemInfo)
                {
                    var vfsi = (VirtualFileSystemInfo) Item;
                    if (vfsi.NoAccess)
                        return Brushes.Red;
                    switch (vfsi.Obfuscation)
                    {
                        case VirtualFileSystemInfo.FileObfuscation.BBTAGEncryption:
                        case VirtualFileSystemInfo.FileObfuscation.FPACEncryption:
                            return Brushes.LimeGreen;
                        case VirtualFileSystemInfo.FileObfuscation.FPACDeflation:
                        case VirtualFileSystemInfo.FileObfuscation.SwitchCompression:
                            return Brushes.Cyan;
                        case VirtualFileSystemInfo.FileObfuscation.FPACEncryption |
                             VirtualFileSystemInfo.FileObfuscation.FPACDeflation:
                            return Brushes.Magenta;
                        default:
                            return null;
                    }
                }

                return null;
            }
        }

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                OnPropertyChanged();
            }
        }

        public bool IsChecked
        {
            get => isChecked;
            set
            {
                isChecked = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }

        public Visibility Visibility
        {
            get => visibility;
            set
            {
                visibility = value;
                OnPropertyChanged();
            }
        }

        public bool IsSpecialFolder { get; set; } = false;

        // Methods
        public void LoadThumbnail()
        {
            if (Item is FileInfo || Item is VirtualFileSystemInfo)
                ShowThumbnail = true;
        }

        // Events

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            var vdi = Item as VirtualDirectoryInfo;
            if (vdi != null)
            {
                if (e.FullPath == vdi.FullName)
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Items.Clear();
                        Items.AddRange(ExploreTools.ExplorePACFiles(this));
                    }));
            }
            else
            {
                if (Items.Count < 1 || Items[0] is DirectoryItemDummy)
                    return;

                if (e.ChangeType == WatcherChangeTypes.Deleted)
                {
                    var matches = Items.Where(i => i.Path == e.FullPath);
                    if (matches.Count() == 0)
                        return;
                    var di = Items.Where(i => i.Path == e.FullPath).First();
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => Items.Remove(di)));
                }

                if (e.ChangeType == WatcherChangeTypes.Created)
                {
                    var directoryItems = new ObservableRangeCollection<DirectoryItem>();
                    directoryItems.AddRange(ExploreTools.ExploreDirectories(this));
                    directoryItems.AddRange(ExploreTools.ExploreFiles(this));
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Items.Clear();
                        Items.AddRange(directoryItems);
                    }));
                }
            }
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            var di = Items.Where(i => i.Path == e.OldFullPath).First();
            di.Path = e.FullPath;
        }

        // INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DirectoryItemDummy : DirectoryItem
    {
        public DirectoryItemDummy()
        {
            Items = new ObservableRangeCollection<DirectoryItem>();
            Name = "Dummy";
            Visibility = Visibility.Hidden;
        }
    }

    public class DirectoryItemMenuItem
    {
        public string Header { get; set; }

        public ICommand Command { get; set; }

        public DirectoryItemMenuItem[] ChildMenuItems { get; set; }
    }
}