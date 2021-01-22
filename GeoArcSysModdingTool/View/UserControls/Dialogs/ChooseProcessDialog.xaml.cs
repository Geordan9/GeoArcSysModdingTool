using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GeoArcSysModdingTool.Utils.Extensions;
using ProcessLib.Models;

namespace GeoArcSysModdingTool.View.UserControls.Dialogs
{
    /// <summary>
    ///     Interaction logic for ProcessWindow.xaml
    /// </summary>
    public partial class ChooseProcessDialog : Window
    {
        public Process myProcess;

        public ChooseProcessDialog()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                Window.DragMove();
        }

        private void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Window.Close();
        }

        private void TitleBarButton_Selected(object sender, RoutedEventArgs e)
        {
            ((ListViewItem) sender).IsSelected = false;
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ChooseProcess(((ProcessSnapshot) ProcessDataGrid.SelectedItem).Process);
        }

        private void DataGridRow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ChooseProcess(((ProcessSnapshot) ProcessDataGrid.SelectedItem).Process);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(ProcessDataGrid.ItemsSource);
            if (view != null)
            {
                view.SortDescriptions.Clear();
                foreach (var column in ProcessDataGrid.Columns) column.SortDirection = null;
            }
        }

        private void ChooseProcess(Process process)
        {
            myProcess = process;
            DialogResult = true;
            Close();
        }

        private void ProcessDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (e.Column.SortDirection == ListSortDirection.Descending)
            {
                ((DataGrid) sender).ClearSort();
                e.Handled = true;
            }
        }
    }
}