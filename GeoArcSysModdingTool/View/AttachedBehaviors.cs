using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace GeoArcSysModdingTool.View
{
    public static class TextChangedBehavior
    {
        public static readonly DependencyProperty TextChangedCommandProperty =
            DependencyProperty.RegisterAttached("TextChangedCommand",
                typeof(ICommand),
                typeof(TextChangedBehavior),
                new UIPropertyMetadata(TextChangedCommandChanged));

        private static readonly DependencyProperty UserInputProperty =
            DependencyProperty.RegisterAttached("UserInput",
                typeof(bool),
                typeof(TextChangedBehavior));

        public static void SetTextChangedCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(TextChangedCommandProperty, value);
        }

        private static void ExecuteTextChangedCommand(FrameworkElement sender, TextChangedEventArgs e)
        {
            var element = sender;
            var cb = sender.TemplatedParent as ComboBox;
            if (cb != null)
            {
                element = cb;
                if (cb.SelectedItem != null)
                    SetUserInput(sender, true);
            }

            var command = (ICommand) element.GetValue(TextChangedCommandProperty);
            var arguments = new[] {GetUserInput(sender), element.DataContext};
            command.Execute(arguments);
        }

        private static bool GetUserInput(DependencyObject target)
        {
            return (bool) target.GetValue(UserInputProperty);
        }

        private static void SetUserInput(DependencyObject target, bool value)
        {
            target.SetValue(UserInputProperty, value);
        }

        private static void TextBoxOnPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command != ApplicationCommands.Cut) return;

            var textBox = sender as TextBox;

            if (textBox == null) return;

            SetUserInput(textBox, true);
        }

        private static void TextBoxOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            switch (e.Key)
            {
                case Key.Return:
                    if (textBox.AcceptsReturn) SetUserInput(textBox, true);
                    break;

                case Key.Delete:
                    if (textBox.SelectionLength > 0 || textBox.SelectionStart < textBox.Text.Length)
                        SetUserInput(textBox, true);
                    break;

                case Key.Back:
                    if (textBox.SelectionLength > 0 || textBox.SelectionStart > 0) SetUserInput(textBox, true);
                    break;
            }
        }

        private static void TextBoxOnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            SetUserInput(textBox, true);
        }

        private static void TextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            ExecuteTextChangedCommand(textBox, e);
            if (!GetUserInput(textBox))
                textBox.CaretIndex = textBox.Text.Length;
            SetUserInput(textBox, false);
        }

        private static void TextBoxOnTextPasted(object sender, DataObjectPastingEventArgs e)
        {
            var textBox = sender as TextBox;
            if (e.SourceDataObject.GetDataPresent(DataFormats.Text, true) == false) return;

            SetUserInput(textBox, true);
        }

        private static void TextBoxOnDoubleClick(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (!textBox.IsReadOnly)
            {
                SetUserInput(textBox, true);
                textBox.Text = string.Empty;
            }
        }

        private static void TextChangedCommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var textBox = target as TextBox;
            var cb = target as ComboBox;
            if (cb != null)
                textBox = (TextBox) cb.Template.FindName("PART_EditableTextBox", cb);

            if (textBox == null) return;

            if (e.OldValue != null)
            {
                textBox.PreviewKeyDown -= TextBoxOnPreviewKeyDown;
                textBox.PreviewTextInput -= TextBoxOnPreviewTextInput;
                CommandManager.RemovePreviewExecutedHandler(textBox, TextBoxOnPreviewExecuted);
                DataObject.RemovePastingHandler(textBox, TextBoxOnTextPasted);
                textBox.TextChanged -= TextBoxOnTextChanged;
                textBox.MouseDoubleClick -= TextBoxOnDoubleClick;
            }

            if (e.NewValue != null)
            {
                textBox.PreviewKeyDown += TextBoxOnPreviewKeyDown;
                textBox.PreviewTextInput += TextBoxOnPreviewTextInput;
                CommandManager.AddPreviewExecutedHandler(textBox, TextBoxOnPreviewExecuted);
                DataObject.AddPastingHandler(textBox, TextBoxOnTextPasted);
                textBox.TextChanged += TextBoxOnTextChanged;
                textBox.MouseDoubleClick += TextBoxOnDoubleClick;
            }
        }
    }

    public static class TreeViewItemExpandedCollapseBehavior
    {
        public static readonly DependencyProperty ExpandCollapseCommandProperty =
            DependencyProperty.RegisterAttached("ExpandCollapseCommand", typeof(ICommand),
                typeof(TreeViewItemExpandedCollapseBehavior),
                new PropertyMetadata(ExpandCollapseCommandChanged));

        public static void SetExpandCollapseCommand(DependencyObject o, ICommand value)
        {
            o.SetValue(ExpandCollapseCommandProperty, value);
        }

        public static ICommand GetExpandCollapseCommand(DependencyObject o)
        {
            return (ICommand) o.GetValue(ExpandCollapseCommandProperty);
        }

        private static void ExecuteExpandCollapseCommand(TreeViewItem sender, RoutedEventArgs e)
        {
            var command = (ICommand) sender.GetValue(ExpandCollapseCommandProperty);
            var arguments = new[] {sender.IsExpanded, sender.DataContext};
            command.Execute(arguments);
        }

        private static void TreeViewItemOnExpandCollapse(object sender, RoutedEventArgs e)
        {
            var tvi = sender as TreeViewItem;
            ExecuteExpandCollapseCommand(tvi, e);
            e.Handled = true;
        }

        private static void ExpandCollapseCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tvi = d as TreeViewItem;
            if (tvi == null) return;

            if (e.OldValue != null)
            {
                tvi.Expanded -= TreeViewItemOnExpandCollapse;
                tvi.Collapsed -= TreeViewItemOnExpandCollapse;
            }

            if (e.NewValue != null)
            {
                tvi.Expanded += TreeViewItemOnExpandCollapse;
                tvi.Collapsed += TreeViewItemOnExpandCollapse;
            }
        }
    }

    public static class TreeViewItemBehavior
    {
        public static readonly DependencyProperty SelectedCommandProperty =
            DependencyProperty.RegisterAttached("SelectedCommand", typeof(ICommand),
                typeof(TreeViewItemBehavior),
                new PropertyMetadata(SelectedCommandChanged));

        public static readonly DependencyProperty MouseLeftButtonUpCommandProperty =
            DependencyProperty.RegisterAttached("MouseLeftButtonUpCommand", typeof(ICommand),
                typeof(TreeViewItemBehavior),
                new PropertyMetadata(MouseLeftButtonUpCommandChanged));

        public static void SetMouseLeftButtonUpCommand(DependencyObject o, ICommand value)
        {
            o.SetValue(MouseLeftButtonUpCommandProperty, value);
        }

        public static ICommand GetMouseLeftButtonUpCommand(DependencyObject o)
        {
            return (ICommand) o.GetValue(MouseLeftButtonUpCommandProperty);
        }

        public static void SetSelectedCommand(DependencyObject o, ICommand value)
        {
            o.SetValue(SelectedCommandProperty, value);
        }

        public static ICommand GetSelectedCommand(DependencyObject o)
        {
            return (ICommand) o.GetValue(SelectedCommandProperty);
        }

        private static void ExecuteMouseLeftButtonUpCommand(TreeViewItem sender, RoutedEventArgs e)
        {
            var command = (ICommand) sender.GetValue(MouseLeftButtonUpCommandProperty);
            var argument = sender.DataContext;
            command.Execute(argument);
        }

        private static void ExecuteSelectedCommand(TreeViewItem sender, RoutedEventArgs e)
        {
            var command = (ICommand) sender.GetValue(SelectedCommandProperty);
            var argument = sender.DataContext;
            command.Execute(argument);
        }

        private static void MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            var tvi = (TreeViewItem) sender;
            if (tvi.IsMouseOver && tvi.IsSelected)
                ExecuteMouseLeftButtonUpCommand(tvi, e);
            e.Handled = true;
        }

        private static void Selected(object sender, RoutedEventArgs e)
        {
            var tvi = (TreeViewItem) sender;
            ExecuteSelectedCommand(tvi, e);
            e.Handled = true;
        }

        private static void MouseLeftButtonUpCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tvi = d as TreeViewItem;
            if (tvi == null) return;

            if (e.OldValue != null) tvi.MouseLeftButtonUp -= MouseLeftButtonUp;

            if (e.NewValue != null) tvi.MouseLeftButtonUp += MouseLeftButtonUp;
        }

        private static void SelectedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tvi = d as TreeViewItem;
            if (tvi == null) return;

            if (e.OldValue != null) tvi.Selected -= Selected;

            if (e.NewValue != null) tvi.Selected += Selected;
        }
    }

    public static class TreeViewHelper
    {
        private static readonly Dictionary<DependencyObject, TreeViewSelectedItemBehavior> behaviors =
            new Dictionary<DependencyObject, TreeViewSelectedItemBehavior>();

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached("SelectedItem", typeof(object), typeof(TreeViewHelper),
                new UIPropertyMetadata(null, SelectedItemChanged));

        public static object GetSelectedItem(DependencyObject obj)
        {
            return obj.GetValue(SelectedItemProperty);
        }

        public static void SetSelectedItem(DependencyObject obj, object value)
        {
            obj.SetValue(SelectedItemProperty, value);
        }

        private static void SelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is TreeView))
                return;

            if (!behaviors.ContainsKey(obj))
                behaviors.Add(obj, new TreeViewSelectedItemBehavior(obj as TreeView));

            var view = behaviors[obj];
            view.ChangeSelectedItem(e.NewValue);
        }

        private class TreeViewSelectedItemBehavior
        {
            private readonly TreeView view;

            public TreeViewSelectedItemBehavior(TreeView view)
            {
                this.view = view;
                view.SelectedItemChanged += (sender, e) => SetSelectedItem(view, e.NewValue);
            }

            internal void ChangeSelectedItem(object p)
            {
                var item = (TreeViewItem) view.ItemContainerGenerator.ContainerFromItem(p);
                if (item != null)
                    item.IsSelected = true;
            }
        }
    }

    public static class ListBoxItemClickBehavior
    {
        public static readonly DependencyProperty MouseDoubleClickCommandProperty =
            DependencyProperty.RegisterAttached("MouseDoubleClickCommand", typeof(ICommand),
                typeof(ListBoxItemClickBehavior),
                new PropertyMetadata(MouseDoubleClickCommandChanged));

        public static void SetMouseDoubleClickCommand(DependencyObject o, ICommand value)
        {
            o.SetValue(MouseDoubleClickCommandProperty, value);
        }

        public static ICommand GetMouseDoubleClickCommand(DependencyObject o)
        {
            return (ICommand) o.GetValue(MouseDoubleClickCommandProperty);
        }

        private static void ExecuteMouseDoubleClickCommand(ListBoxItem sender, RoutedEventArgs e)
        {
            var command = (ICommand) sender.GetValue(MouseDoubleClickCommandProperty);
            var argument = sender.DataContext;
            command.Execute(argument);
        }

        private static void MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            var lbi = (ListBoxItem) sender;
            if (lbi.IsSelected)
                ExecuteMouseDoubleClickCommand(lbi, e);
        }

        private static void MouseDoubleClickCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var lbi = d as ListBoxItem;
            if (lbi == null) return;

            if (e.OldValue != null) lbi.MouseDoubleClick -= MouseDoubleClick;

            if (e.NewValue != null) lbi.MouseDoubleClick += MouseDoubleClick;
        }
    }

    public static class ScrollToTopBehavior
    {
        public static readonly DependencyProperty ScrollToTopProperty =
            DependencyProperty.RegisterAttached
            (
                "ScrollToTop",
                typeof(bool),
                typeof(ScrollToTopBehavior),
                new UIPropertyMetadata(false, OnScrollToTopPropertyChanged)
            );

        public static bool GetScrollToTop(DependencyObject obj)
        {
            return (bool) obj.GetValue(ScrollToTopProperty);
        }

        public static void SetScrollToTop(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollToTopProperty, value);
        }

        private static void OnScrollToTopPropertyChanged(DependencyObject dpo,
            DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = dpo as ItemsControl;
            if (itemsControl != null)
            {
                var dependencyPropertyDescriptor =
                    DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ItemsControl));
                if (dependencyPropertyDescriptor != null)
                {
                    if ((bool) e.NewValue)
                        dependencyPropertyDescriptor.AddValueChanged(itemsControl, ItemsSourceChanged);
                    else
                        dependencyPropertyDescriptor.RemoveValueChanged(itemsControl, ItemsSourceChanged);
                }
            }
        }

        private static void ItemsSourceChanged(object sender, EventArgs e)
        {
            var itemsControl = sender as ItemsControl;
            EventHandler eventHandler = null;
            eventHandler = delegate
            {
                if (itemsControl.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    var scrollViewer = GetVisualChild<ScrollViewer>(itemsControl);
                    scrollViewer.ScrollToTop();
                    itemsControl.ItemContainerGenerator.StatusChanged -= eventHandler;
                }
            };
            itemsControl.ItemContainerGenerator.StatusChanged += eventHandler;
        }

        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            var child = default(T);
            var numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < numVisuals; i++)
            {
                var v = (Visual) VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null) child = GetVisualChild<T>(v);
                if (child != null) break;
            }

            return child;
        }
    }
}