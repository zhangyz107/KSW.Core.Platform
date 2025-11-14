using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace KSW.UI.WPF.Controls
{
    [TemplatePart(Name = PART_ScrollViewer, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
    [TemplatePart(Name = PART_SimpleWrapPanel, Type = typeof(Panel))]
    [TemplatePart(Name = PART_DropDownPanel, Type = typeof(Panel))]
    public class MultiComboBox : ListBox
    {
        #region Private
        private const string PART_ScrollViewer = "PART_ScrollViewer";
        private const string PART_Popup = "PART_Popup";
        private const string PART_SimpleWrapPanel = "PART_SimpleWrapPanel";
        private const string PART_DropDownPanel = "PART_DropDown";

        private HwndSource _hwndSource;
        private Window _window;
        private Popup _popup;
        private Panel _panel;
        private Panel _panelDropDown;
        private bool _ignoreTextValueChanged;
        #endregion

        #region Dependency Properties

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public new static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IList), typeof(MultiComboBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemsChanged));

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        public static readonly DependencyProperty IsDropDownOpenProperty =
    DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(MultiComboBox),
    new PropertyMetadata(false));

        public static readonly DependencyProperty MaxDropDownHeightProperty
    = DependencyProperty.Register("MaxDropDownHeight", typeof(double), typeof(MultiComboBox),
        new PropertyMetadata(SystemParameters.PrimaryScreenHeight / 3));
        #endregion

        #region Properties
        public new IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public bool IsDropDownOpen
        {
            get => (bool)GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        [Bindable(true)]
        [Category("Layout")]
        [TypeConverter(typeof(LengthConverter))]
        public double MaxDropDownHeight
        {
            get => (double)GetValue(MaxDropDownHeightProperty);
            set => SetValue(MaxDropDownHeightProperty, value);
        }
        #endregion

        static MultiComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiComboBox),
                new FrameworkPropertyMetadata(typeof(MultiComboBox)));
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MultiComboBoxItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MultiComboBoxItem();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (SelectedItems != null)
            {
                foreach (var item in e.AddedItems)
                {
                    if (!SelectedItems.Contains(item))
                    {
                        SelectedItems.Add(item);
                    }
                }
                foreach (var item in e.RemovedItems)
                {
                    SelectedItems.Remove(item);
                }
            }

            UpdateText();
        }

        protected virtual void UpdateText()
        {
            if (_ignoreTextValueChanged) return;

            _ignoreTextValueChanged = true;
            UpdateTags();
            _ignoreTextValueChanged = false;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _window = Window.GetWindow(this);
            if (_window != null)
            {
                if (_window.IsInitialized)
                    WindowSourceInitialized(_window, EventArgs.Empty);
                else
                {
                    _window.SourceInitialized -= WindowSourceInitialized;
                    _window.SourceInitialized += WindowSourceInitialized;
                }
            }

            _panelDropDown = GetTemplateChild(PART_DropDownPanel) as Panel;

            // 获取Popup控件
            _popup = GetTemplateChild(PART_Popup) as Popup;
            if (_popup != null && _window != null)
            {
                _popup.Closed += OnPopup_Closed;
                _popup.Closed -= OnPopup_Closed;
                _popup.Opened -= OnPopup_Opened;
                _popup.Opened += OnPopup_Opened;
            }

            AddHandler(ClosableTag.CloseEvent, new RoutedEventHandler(Tags_Close));
            _panel = GetTemplateChild(PART_SimpleWrapPanel) as Panel;

        }

        private void OnPopup_Closed(object? sender, EventArgs e)
        {
            _window.PreviewMouseDown -= OnWindowPreviewMouseDown;
        }

        private void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseOver)
                IsDropDownOpen = false;
        }

        private void OnPopup_Opened(object? sender, EventArgs e)
        {
            _window.PreviewMouseDown += OnWindowPreviewMouseDown;
            UpdateTags();
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                if (_popup == null) return;

                double popupHeight = _panelDropDown.ActualHeight;
                var controlScreenPos = PointToScreen(new Point(0, RenderSize.Height));
                var window = Window.GetWindow(this);
                if (window == null) return;

                var windowScreenPos = window.PointToScreen(new Point(0, 0));
                double availableBottomSpace = (windowScreenPos.Y + window.ActualHeight) - controlScreenPos.Y;
                double availableTopSpace = controlScreenPos.Y - windowScreenPos.Y;
                if (availableBottomSpace < popupHeight && availableTopSpace > popupHeight)
                {
                    _popup.Placement = PlacementMode.Top;
                    _popup.VerticalOffset = -popupHeight - 2;
                }
                else
                {
                    _popup.Placement = PlacementMode.Bottom;
                    _popup.VerticalOffset = 0;
                }
            }));

        }

        private void Tags_Close(object sender, RoutedEventArgs e)
        {
            var tag = (ClosableTag)e.OriginalSource;
            var multiComboBoxItem = (MultiComboBoxItem)tag.Tag;
            if (multiComboBoxItem != null)
                multiComboBoxItem.SetCurrentValue(IsSelectedProperty, false);
        }

        private void UpdateTags()
        {
            if (_panel == null) return;
            _panel.Children.Clear();
            foreach (var item in SelectedItems)
            {
                if (ItemContainerGenerator.ContainerFromItem(item) is MultiComboBoxItem multiComboBoxItem)
                    CreateTag(item, multiComboBoxItem);
                else
                    CreateTag(item);
            }
        }

        private void CreateTag(object item, MultiComboBoxItem multiComboBoxItem = null)
        {
            var tag = new ClosableTag { Padding = new Thickness(2, 0, 0, 0) };
            if (ItemsSource != null)
            {
                var binding = new Binding(DisplayMemberPath) { Source = item };
                tag.SetBinding(ContentControl.ContentProperty, binding);
            }
            else
            {
                if (multiComboBoxItem != null)
                    tag.Content = multiComboBoxItem.Content;
            }
            if (multiComboBoxItem != null)
                tag.Tag = multiComboBoxItem;
            tag.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
            _panel.Children.Add(tag);
        }

        private void WindowSourceInitialized(object sender, EventArgs empty)
        {
            var window = sender as Window;
            if (window != null)
            {
                _hwndSource = PresentationSource.FromVisual(window) as HwndSource;
                if (_hwndSource != null)
                {
                    _hwndSource.AddHook(WndProc);
                }
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_NCLBUTTONDOWN = 0x00A1;
            if (msg == WM_NCLBUTTONDOWN)
            {
                IsDropDownOpen = false;
            }
            return IntPtr.Zero;
        }
    }
}
