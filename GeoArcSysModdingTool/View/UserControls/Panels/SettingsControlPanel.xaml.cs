using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GeoArcSysModdingTool.Utils;

namespace GeoArcSysModdingTool.View.UserControls.Panels
{
    public partial class SettingsControlPanel : UserControl
    {
        private static string oldText;

        private static Brush oldBrush;

        public SettingsControlPanel()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingTools.UpdateSettings();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox) sender;

            oldText = textBox.Text;

            oldBrush = textBox.Foreground;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var textBox = (TextBox) sender;

            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                if (e.Key == Key.Escape) textBox.Text = oldText;

                var ancestor = textBox.Parent;
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

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox) sender;
            if (string.IsNullOrWhiteSpace(textBox.Text) || !TypeTools.IsUnsignedIntFormat(textBox.Text))
                textBox.Text = oldText;

            oldText = null;

            textBox.Foreground = oldBrush;

            textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();

            SettingTools.UpdateSettings();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (oldText != null)
                ((TextBox) sender).Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00));
        }
    }
}