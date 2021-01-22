using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArcSysAPI.Models;
using GeoArcSysModdingTool.Models;

namespace GeoArcSysModdingTool.View.UserControls.Dialogs
{
    /// <summary>
    ///     Interaction logic for PACItemSelectorDialog.xaml
    /// </summary>
    public partial class PACItemSelectorDialog : Window, INotifyPropertyChanged
    {
        private DirectoryItem[] _Files;
        private string _selectButtonText = "Select";

        public VirtualFileSystemInfo SelectedItem;

        public PACItemSelectorDialog(PACFileInfo pf)
        {
            InitializeComponent();
            Files = VirtualFilesToDirectoryItems(pf.GetFiles());
        }

        public DirectoryItem[] Files
        {
            get => _Files;
            set
            {
                _Files = value;
                OnPropertyChanged();
            }
        }

        public string selectButtonText
        {
            get => _selectButtonText;
            set
            {
                _selectButtonText = value;
                OnPropertyChanged();
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                Window.DragMove();
        }

        private void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void TitleBarButton_Selected(object sender, RoutedEventArgs e)
        {
            ((ListViewItem) sender).IsSelected = false;
        }

        private DirectoryItem[] VirtualFilesToDirectoryItems(VirtualFileSystemInfo[] files)
        {
            var dirItems = new DirectoryItem[files.Length];
            for (var i = 0; i < files.Length; i++)
                dirItems[i] = new DirectoryItem(files[i])
                {
                    Name = files[i].Name,
                    Item = files[i]
                };

            return dirItems;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectItem((DirectoryItem) ((ListBoxItem) sender).DataContext);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectItem((DirectoryItem) PACItemsListBox.SelectedItem);
        }

        private void SelectItem(DirectoryItem di)
        {
            SelectedItem = (VirtualFileSystemInfo) di.Item;
            if (SelectedItem is PACFileInfo)
            {
                selectButtonText = "Select";
                Files = VirtualFilesToDirectoryItems(((PACFileInfo) SelectedItem).GetFiles());
                return;
            }

            DialogResult = true;
            Close();
        }

        private void PACItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ((ListBox) sender).SelectedItem;
            if (item == null)
                return;
            var di = (DirectoryItem) item;
            if (di.Item is PACFileInfo)
                selectButtonText = "Open";
            else
                selectButtonText = "Select";
        }

        // INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}