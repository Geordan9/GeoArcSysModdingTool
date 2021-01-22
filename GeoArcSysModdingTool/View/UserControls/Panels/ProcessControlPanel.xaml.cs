using System.Windows;
using System.Windows.Controls;
using GeoArcSysModdingTool.Utils;
using GeoArcSysModdingTool.View.UserControls.Dialogs;

namespace GeoArcSysModdingTool.View.UserControls.Panels
{
    public partial class ProcessControlPanel : UserControl
    {
        public ProcessControlPanel()
        {
            InitializeComponent();
        }

        private void SelectProcessButton_Click(object sender, RoutedEventArgs e)
        {
            var processWindow = new ChooseProcessDialog();
            processWindow.Owner = Window.GetWindow(this);
            ChangeWindowOpacity(0.6);
            var result = processWindow.ShowDialog();
            if (result == true) Mediator.NotifyColleagues("SelectProcess", processWindow.myProcess);
            ChangeWindowOpacity(1.0);
        }

        private void ChangeWindowOpacity(double opacity)
        {
            Mediator.NotifyColleagues("ChangeWindowOpacity", opacity);
        }
    }
}