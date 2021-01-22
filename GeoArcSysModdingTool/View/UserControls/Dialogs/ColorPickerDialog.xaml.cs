using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace GeoArcSysModdingTool.View.UserControls.Dialogs
{
    /// <summary>
    ///     Interaction logic for ColorPickerDialog.xaml
    /// </summary>
    public partial class ColorPickerDialog : Window
    {
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            "SelectedColor", typeof(Color?), typeof(ColorPickerDialog));

        public static readonly DependencyProperty UseAlphaChannelProperty = DependencyProperty.Register(
            "UseAlphaChannel", typeof(bool), typeof(ColorPickerDialog));

        public ColorPickerDialog()
        {
            InitializeComponent();
        }

        public Color? SelectedColor
        {
            get => (Color?) GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public bool UseAlphaChannel
        {
            get => (bool) GetValue(UseAlphaChannelProperty);
            set => SetValue(UseAlphaChannelProperty, value);
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

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            UpdateTextBox((TextBox) sender, e.Text);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var textBox = (TextBox) sender;
            BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty).UpdateSource();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            var key = GetCharFromKey(e.Key).ToString();
            /*var num = 0;
            if (!int.TryParse(key, out num)) return;*/
            UpdateTextBox((TextBox) sender, key);
        }

        private void UpdateTextBox(TextBox textBox, string newText)
        {
            var origText = textBox.Text;
            var caretIndex = textBox.CaretIndex;
            var index = textBox.SelectionStart;
            var tmpText = origText.Remove(index, textBox.SelectionLength);

            var str = string.Empty;

            var backSpace = newText == "\b";

            var isHex = origText.Contains("#");

            if (isHex)
            {
                if (!backSpace)
                {
                    newText = newText.ToUpper();
                    var rplIndex = caretIndex >= origText.Length ? origText.Length - 1 : caretIndex;
                    str = origText.Remove(rplIndex, 1);
                    str = str.Insert(rplIndex, newText);
                    if (string.IsNullOrWhiteSpace(str))
                        str = "0";

                    if (!str.Contains("#"))
                    {
                        textBox.CaretIndex++;
                        return;
                    }

                    caretIndex++;

                    if (caretIndex > str.Length) return;
                }
            }
            else if (!string.IsNullOrWhiteSpace(newText))
            {
                if (!backSpace)
                {
                    var before = tmpText.Substring(0, index);
                    str = before +
                          newText +
                          tmpText.Substring(index);

                    caretIndex = before.Length + newText.Length;
                }
                else
                {
                    if (caretIndex == 0)
                        return;
                    var length = textBox.SelectionLength > 0 ? textBox.SelectionLength : 1;
                    str = origText.Remove(caretIndex >= origText.Length ? origText.Length - 1 : caretIndex - 1, length);
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        str = "0";
                        caretIndex++;
                    }
                    else
                    {
                        caretIndex--;
                    }
                }
            }
            else
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(str)) return;

            var num = 0;
            if (!isHex)
            {
                if (!int.TryParse(str, out num)) return;
                if (num > byte.MaxValue) return;
            }
            else
            {
                if (!int.TryParse(str.Substring(1), NumberStyles.HexNumber, null, out num)) return;
                if (num > int.MaxValue) return;
            }

            textBox.Text = str;
            var bindExpress = BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty);
            if (bindExpress != null)
                bindExpress.UpdateSource();
            textBox.CaretIndex = str.Length < caretIndex ? str.Length : caretIndex;
        }

        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3
        }

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out] [MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        public static char GetCharFromKey(Key key)
        {
            var ch = ' ';

            var virtualKey = KeyInterop.VirtualKeyFromKey(key);
            var keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            var scanCode = MapVirtualKey((uint) virtualKey, MapType.MAPVK_VK_TO_VSC);
            var stringBuilder = new StringBuilder(2);

            var result = ToUnicode((uint) virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity,
                0);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                case 1:
                {
                    ch = stringBuilder[0];
                    break;
                }
                default:
                {
                    ch = stringBuilder[0];
                    break;
                }
            }

            return ch;
        }
    }
}