using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GeoArcSysModdingTool.Models;

namespace GeoArcSysModdingTool.View.UserControls.Panels
{
    /// <summary>
    ///     Interaction logic for ExplorerNavBar.xaml
    /// </summary>
    public partial class ExplorerNavBar : UserControl
    {
        public static readonly DependencyProperty GoBackCommandProperty = DependencyProperty.Register(
            "GoBackCommand", typeof(ICommand), typeof(ExplorerNavBar));

        public static readonly DependencyProperty GoForwardCommandProperty = DependencyProperty.Register(
            "GoForwardCommand", typeof(ICommand), typeof(ExplorerNavBar));

        public static readonly DependencyProperty GoUpCommandProperty = DependencyProperty.Register(
            "GoUpCommand", typeof(ICommand), typeof(ExplorerNavBar));

        public static readonly DependencyProperty CurDirItemProperty = DependencyProperty.Register(
            "CurDirItem", typeof(DirectoryItem), typeof(ExplorerNavBar));

        public ExplorerNavBar()
        {
            InitializeComponent();
        }

        public ICommand GoBackCommand
        {
            get => (ICommand) GetValue(GoBackCommandProperty);
            set => SetValue(GoBackCommandProperty, value);
        }

        public ICommand GoForwardCommand
        {
            get => (ICommand) GetValue(GoForwardCommandProperty);
            set => SetValue(GoForwardCommandProperty, value);
        }

        public ICommand GoUpCommand
        {
            get => (ICommand) GetValue(GoUpCommandProperty);
            set => SetValue(GoUpCommandProperty, value);
        }

        public DirectoryItem CurDirItem
        {
            get => (DirectoryItem) GetValue(CurDirItemProperty);
            set => SetValue(CurDirItemProperty, value);
        }
    }
}