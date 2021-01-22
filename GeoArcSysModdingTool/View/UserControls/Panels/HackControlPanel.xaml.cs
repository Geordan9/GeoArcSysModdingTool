using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GeoArcSysModdingTool.View.UserControls.Panels
{
    public partial class HackControlPanel : UserControl
    {
        private string oldValue = string.Empty;

        public HackControlPanel()
        {
            InitializeComponent();
        }

        private void HackInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                if (e.Key == Key.Escape) SetProperty(sender.GetType(), sender, "Text", oldValue);

                var selement = (FrameworkElement) sender;
                var ancestor = selement.Parent;
                if (ancestor == null)
                    ancestor = selement.TemplatedParent;
                while (ancestor != null)
                {
                    var element = ancestor as UIElement;
                    if (element != null && element.Focusable)
                    {
                        element.Focus();
                        break;
                    }

                    ancestor = VisualTreeHelper.GetParent(ancestor);
                }
            }
        }

        private void HackInput_GotFocus(object sender, RoutedEventArgs e)
        {
            oldValue = (string) GetPropertyValue(sender.GetType(), sender, "Text");
        }

        private void HackInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace((string) GetPropertyValue(sender.GetType(), sender, "Text")))
                SetProperty(sender.GetType(), sender, "Text", oldValue);
        }

        public void SetProperty(Type t, object sender, string property, object value)
        {
            t.GetProperty(property).SetValue(sender, value, null);
        }

        public object GetPropertyValue(Type t, object sender, string property)
        {
            return t.GetProperty(property).GetValue(sender, null);
        }

        private void HackInput_DropDownClosed(object sender, EventArgs e)
        {
            HackInput_LostFocus(sender, null);
        }
    }
}