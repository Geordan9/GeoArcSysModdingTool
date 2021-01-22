using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GeoArcSysModdingTool.Models;
using GeoArcSysModdingTool.Utils;

namespace GeoArcSysModdingTool.View.UserControls.Panels
{
    /// <summary>
    ///     Interaction logic for TreeViewExplorer.xaml
    /// </summary>
    public partial class TreeViewExplorer : UserControl
    {
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem", typeof(DirectoryItem), typeof(TreeViewExplorer));

        public static readonly DependencyProperty RootDirItemsProperty = DependencyProperty.Register(
            "RootDirItems", typeof(ObservableRangeCollection<DirectoryItem>), typeof(TreeViewExplorer),
            new FrameworkPropertyMetadata(null, OnRootDirItemsChanged));

        public static readonly DependencyProperty SelectImageCommandProperty = DependencyProperty.Register(
            "SelectImageCommand", typeof(ICommand), typeof(TreeViewExplorer));

        public static readonly DependencyProperty ExpandCollapseCommandProperty = DependencyProperty.Register(
            "ExpandCollapseCommand", typeof(ICommand), typeof(TreeViewExplorer));

        public static readonly DependencyProperty OpenDirectoryCommandProperty = DependencyProperty.Register(
            "OpenDirectoryCommand", typeof(ICommand), typeof(TreeViewExplorer));

        public static readonly DependencyProperty ToggleDirectoryItemCommandProperty = DependencyProperty.Register(
            "ToggleDirectoryItemCommand", typeof(ICommand), typeof(TreeViewExplorer));

        public static readonly DependencyProperty UseCheckBoxProperty = DependencyProperty.Register(
            "UseCheckbox", typeof(bool), typeof(TreeViewExplorer), new PropertyMetadata(false, null));

        public static readonly DependencyProperty UseVirtualizationProperty = DependencyProperty.Register(
            "UseVirtualization", typeof(bool), typeof(TreeViewExplorer), new PropertyMetadata(false, null));

        private static void OnRootDirItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tve = (TreeViewExplorer) d;
            tve.TreeView.ItemsSource = (ObservableRangeCollection<DirectoryItem>) e.NewValue;
        }

        public TreeViewExplorer()
        {
            InitializeComponent();
        }

        public DirectoryItem SelectedItem
        {
            get => (DirectoryItem) GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public ObservableRangeCollection<DirectoryItem> RootDirItems
        {
            get => (ObservableRangeCollection<DirectoryItem>) GetValue(RootDirItemsProperty);
            set => SetValue(RootDirItemsProperty, value);
        }

        public ICommand SelectImageCommand
        {
            get => (ICommand) GetValue(SelectImageCommandProperty);
            set => SetValue(SelectImageCommandProperty, value);
        }

        public ICommand ExpandCollapseCommand
        {
            get => (ICommand) GetValue(ExpandCollapseCommandProperty);
            set => SetValue(ExpandCollapseCommandProperty, value);
        }

        public ICommand OpenDirectoryCommand
        {
            get => (ICommand) GetValue(OpenDirectoryCommandProperty);
            set => SetValue(OpenDirectoryCommandProperty, value);
        }

        public ICommand ToggleDirectoryItemCommand
        {
            get => (ICommand) GetValue(ToggleDirectoryItemCommandProperty);
            set => SetValue(ToggleDirectoryItemCommandProperty, value);
        }

        public bool UseCheckBox
        {
            get => (bool) GetValue(UseCheckBoxProperty);
            set => SetValue(UseCheckBoxProperty, value);
        }

        public bool UseVirtualization
        {
            get => (bool) GetValue(UseVirtualizationProperty);
            set => SetValue(UseVirtualizationProperty, value);
        }

        private void DirectoryItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var dirItem = (DirectoryItem) ((FrameworkElement) sender).DataContext;
            if (!dirItem.ContextMenuHasOpened)
                Mediator.NotifyColleagues("UpdateContextMenu", dirItem);
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = (DirectoryItem) TreeView.SelectedItem;
        }

        private void TreeViewExplorerControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
        }
    }
}