﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GeoArcSysModdingTool.View.CustomControls
{
    public class ZoomBorder : Border
    {
        public static readonly DependencyProperty WillResetProperty = DependencyProperty.Register(
            "WillReset", typeof(bool), typeof(ZoomBorder),
            new FrameworkPropertyMetadata(OnWillResetChanged));

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
            "Zoom", typeof(double), typeof(ZoomBorder), new PropertyMetadata(0.2, null));

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale", typeof(double), typeof(ZoomBorder), new PropertyMetadata(0.4, null));

        private UIElement child;
        private Point origin;
        private Point start;

        public override UIElement Child
        {
            get => base.Child;
            set
            {
                if (value != null && value != Child)
                    Initialize(value);
                base.Child = value;
            }
        }

        public bool WillReset
        {
            get => (bool) GetValue(WillResetProperty);
            set => SetValue(WillResetProperty, false);
        }

        public double Zoom
        {
            get => (double) GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        public double Scale
        {
            get => (double) GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform) ((TransformGroup) element.RenderTransform)
                .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform) ((TransformGroup) element.RenderTransform)
                .Children.First(tr => tr is ScaleTransform);
        }

        public void Initialize(UIElement element)
        {
            child = element;
            if (child != null)
            {
                var group = new TransformGroup();
                var st = new ScaleTransform();
                group.Children.Add(st);
                var tt = new TranslateTransform();
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);
                MouseWheel += child_MouseWheel;
                MouseLeftButtonDown += child_MouseLeftButtonDown;
                MouseLeftButtonUp += child_MouseLeftButtonUp;
                MouseMove += child_MouseMove;
            }
        }

        private static void OnWillResetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zb = (ZoomBorder) d;
            if ((bool) e.NewValue)
            {
                zb.Reset();
                zb.WillReset = false;
            }
        }

        public void Reset()
        {
            if (child != null)
            {
                // reset zoom
                var st = GetScaleTransform(child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                var tt = GetTranslateTransform(child);
                tt.X = 0.0;
                tt.Y = 0.0;

                child.ReleaseMouseCapture();
            }
        }

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                var zoom = e.Delta > 0 ? Zoom : -Zoom;
                if (!(e.Delta > 0) && (st.ScaleX < Scale || st.ScaleY < Scale))
                    return;

                var relative = e.GetPosition(child);
                double absoluteX;
                double absoluteY;

                absoluteX = relative.X * st.ScaleX + tt.X;
                absoluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;

                tt.X = absoluteX - relative.X * st.ScaleX;
                tt.Y = absoluteY - relative.Y * st.ScaleY;
            }
        }

        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (child != null && !child.IsMouseCaptured)
            {
                var tt = GetTranslateTransform(child);
                start = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                //Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
                child.ReleaseMouseCapture();
            //Cursor = Cursors.Arrow;
        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
                if (child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(child);
                    var v = start - e.GetPosition(this);
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;
                }
        }

        #endregion
    }
}