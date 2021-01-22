using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GeoArcSysModdingTool.Utils;
using MaterialDesignThemes.Wpf;
using WinInterop = System.Windows.Interop;

namespace GeoArcSysModdingTool.View
{
    public partial class MainWindow : Window
    {
        private bool needsToResize;

        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += win_SourceInitialized;
            SettingTools.UpdateSettings();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                if (e.ClickCount == 2)
                {
                    AdjustWindowSize();
                }
                else
                {
                    if (WindowState == WindowState.Maximized) needsToResize = true;
                    DragMove();
                }
        }

        private void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MaximizeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AdjustWindowSize();
        }

        private void MinimizeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void TitleBarButton_Selected(object sender, RoutedEventArgs e)
        {
            ((ListViewItem) sender).IsSelected = false;
        }

        private void AdjustWindowSize()
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            MaxButton.Kind = WindowState == WindowState.Maximized
                ? PackIconKind.WindowRestore
                : PackIconKind.WindowMaximize;
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (needsToResize)
            {
                needsToResize = false;

                var percentHorizontal = e.GetPosition(this).X / ActualWidth;
                var targetHorizontal = RestoreBounds.Width * percentHorizontal;

                var percentVertical = e.GetPosition(this).Y / ActualHeight;
                var targetVertical = RestoreBounds.Height * percentVertical;

                WindowState = WindowState.Normal;

                POINT lMousePosition;
                GetCursorPos(out lMousePosition);

                Left = lMousePosition.X - targetHorizontal;
                Top = lMousePosition.Y - targetVertical;

                if (e.LeftButton == MouseButtonState.Pressed) DragMove();
            }
        }

        private void TitleBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            needsToResize = false;
        }

        private static IntPtr WindowProc(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (IntPtr) 0;
        }

        private void win_SourceInitialized(object sender, EventArgs e)
        {
            var handle = new WinInterop.WindowInteropHelper(this).Handle;
            WinInterop.HwndSource.FromHwnd(handle).AddHook(WindowProc);
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            var mmi = (MINMAXINFO) Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            var MONITOR_DEFAULTTONEAREST = 0x00000002;
            var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                var rcWorkArea = monitorInfo.rcWork;
                var rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMinTrackSize.X = 600;
                mmi.ptMinTrackSize.Y = 480;
                mmi.ptMaxTrackSize.X = Math.Abs(rcWorkArea.Width);
                mmi.ptMaxTrackSize.Y = Math.Abs(rcWorkArea.Height);
                mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.X = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        private void win_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            public RECT rcMonitor = new RECT();

            public RECT rcWork = new RECT();

            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public static readonly RECT Empty;

            public int Width => Math.Abs(right - left);

            public int Height => bottom - top;

            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public RECT(RECT rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }

            public bool IsEmpty => left >= right || top >= bottom;

            public override string ToString()
            {
                if (this == Empty) return "RECT {Empty}";
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom +
                       " }";
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) return false;
                return this == (RECT) obj;
            }

            public override int GetHashCode()
            {
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }

            public static bool operator ==(RECT rect1, RECT rect2)
            {
                return rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right &&
                       rect1.bottom == rect2.bottom;
            }

            public static bool operator !=(RECT rect1, RECT rect2)
            {
                return !(rect1 == rect2);
            }
        }
    }
}