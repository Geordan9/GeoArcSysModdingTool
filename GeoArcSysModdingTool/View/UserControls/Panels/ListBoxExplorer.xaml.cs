using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GeoArcSysModdingTool.Models;
using GeoArcSysModdingTool.Utils;

namespace GeoArcSysModdingTool.View.UserControls.Panels
{
    /// <summary>
    ///     Interaction logic for ListBoxExplorer.xaml
    /// </summary>
    public partial class ListBoxExplorer : UserControl
    {
        public static readonly DependencyProperty CurDirItemProperty = DependencyProperty.Register(
            "CurDirItem", typeof(DirectoryItem), typeof(ListBoxExplorer),
            new FrameworkPropertyMetadata(null, OnCurDirItemChanged));

        public static readonly DependencyProperty OpenDirectoryItemCommandProperty = DependencyProperty.Register(
            "OpenDirectoryItemCommand", typeof(ICommand), typeof(ListBoxExplorer));

        private static void OnCurDirItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var lbe = (ListBoxExplorer) d;
            lbe.ListBox.DataContext = (DirectoryItem) e.NewValue;
        }

        public ListBoxExplorer()
        {
            InitializeComponent();
        }

        public DirectoryItem CurDirItem
        {
            get => (DirectoryItem) GetValue(CurDirItemProperty);
            set => SetValue(CurDirItemProperty, value);
        }

        public ICommand OpenDirectoryItemCommand
        {
            get => (ICommand) GetValue(OpenDirectoryItemCommandProperty);
            set => SetValue(OpenDirectoryItemCommandProperty, value);
        }

        private void DirectoryItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var dirItem = (DirectoryItem) ((FrameworkElement) sender).DataContext;
            if (!dirItem.ContextMenuHasOpened)
                Mediator.NotifyColleagues("UpdateContextMenu", dirItem);
        }

        private void ListBoxExplorerControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
        }
    }
}