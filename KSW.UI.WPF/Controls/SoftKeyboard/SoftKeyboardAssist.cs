using KSW.Helpers;
using KSW.UI.WPF.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace KSW.UI.WPF.Controls
{
    public static class SoftKeyboardAssist
    {
        /// <summary>
        /// 是否使用软键盘
        /// </summary>
        public static readonly DependencyProperty UseNumericSoftKeyboardProperty = DependencyProperty.RegisterAttached(
            "UseNumericSoftKeyboard",
            typeof(bool),
            typeof(SoftKeyboardAssist),
            new PropertyMetadata(false, OnUseNumericSoftKeyboardChanged));

        public static bool GetUseSoftKeyboard(DependencyObject obj)
        {
            return (bool)obj.GetValue(UseNumericSoftKeyboardProperty);
        }

        public static void SetUseSoftKeyboard(DependencyObject obj, bool value)
        {
            obj.SetValue(UseNumericSoftKeyboardProperty, value);
        }

        /// <summary>
        /// 软键盘模式
        /// </summary>
        public static readonly DependencyProperty ModeProperty = DependencyProperty.RegisterAttached(
            "Mode",
            typeof(SoftKeyboardMode),
            typeof(SoftKeyboardAssist),
            new PropertyMetadata(SoftKeyboardMode.Multiple, OnModeChanged));

        public static SoftKeyboardMode GetMode(DependencyObject obj) => (SoftKeyboardMode)obj.GetValue(ModeProperty);
        public static void SetMode(DependencyObject obj, SoftKeyboardMode value) => obj.SetValue(ModeProperty, value);

        /// <summary>
        /// 软键盘标题
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.RegisterAttached(
            "Title",
            typeof(string),
            typeof(SoftKeyboardAssist),
            new PropertyMetadata(null, OnTitleChanged));

        public static string GetTitle(DependencyObject obj) => (string)obj.GetValue(TitleProperty);
        public static void SetTitle(DependencyObject obj, string value) => obj.SetValue(TitleProperty, value);

        /// <summary>
        /// 上键命令
        /// </summary>
        public static readonly DependencyProperty UpKeyCommandProperty = DependencyProperty.RegisterAttached(
        "UpKeyCommand",
        typeof(ICommand),
        typeof(SoftKeyboardAssist),
        new PropertyMetadata(null, OnUpKeyCommandChanged));

        public static ICommand GetUpKeyCommand(DependencyObject obj) => (ICommand)obj.GetValue(UpKeyCommandProperty);
        public static void SetUpKeyCommand(DependencyObject obj, ICommand value) => obj.SetValue(UpKeyCommandProperty, value);

        /// <summary>
        /// 下键命令
        /// </summary>
        public static readonly DependencyProperty DownKeyCommandProperty = DependencyProperty.RegisterAttached(
        "DownKeyCommand",
        typeof(ICommand),
        typeof(SoftKeyboardAssist),
        new PropertyMetadata(null, OnDownKeyCommandChanged));

        public static ICommand GetDownKeyCommand(DependencyObject obj) => (ICommand)obj.GetValue(DownKeyCommandProperty);
        public static void SetDownKeyCommand(DependencyObject obj, ICommand value) => obj.SetValue(DownKeyCommandProperty, value);

        /// <summary>
        /// 单位(允许接受集合)
        /// </summary>
        public static readonly DependencyProperty UnitsProperty = DependencyProperty.RegisterAttached(
        "Units",
        typeof(IList),
        typeof(SoftKeyboardAssist),
        new PropertyMetadata(null, OnUnitsChanged));

        public static IList GetUnits(DependencyObject obj) => (IList)obj.GetValue(UnitsProperty);
        public static void SetUnits(DependencyObject obj, IList value) => obj.SetValue(UnitsProperty, value);

        /// <summary>
        /// 单位步进值
        /// </summary>
        public static readonly DependencyProperty StepProperty = DependencyProperty.RegisterAttached(
            "Step",
            typeof(double),
            typeof(SoftKeyboardAssist),
            new PropertyMetadata(0.0, OnStepChanged));

        public static double GetStep(DependencyObject obj) => (double)obj.GetValue(StepProperty);
        public static void SetStep(DependencyObject obj, double value) => obj.SetValue(StepProperty, value);

        /// <summary>
        /// 显示值路径
        /// </summary>
        public static readonly DependencyProperty DisplayValuePathProperty = DependencyProperty.RegisterAttached(
            "DisplayValuePath",
            typeof(string),
            typeof(SoftKeyboardAssist),
            new PropertyMetadata(null, OnDisplayValuePathChanged));

        public static string GetDisplayValuePath(DependencyObject obj) => (string)obj.GetValue(DisplayValuePathProperty);
        public static void SetDisplayValuePath(DependencyObject obj, string value) => obj.SetValue(DisplayValuePathProperty, value);

        /// <summary>
        /// 键盘背景色
        /// </summary>
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.RegisterAttached(
            "Background",
            typeof(SolidColorBrush),
            typeof(SoftKeyboardAssist),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x13,0x15,0x1b)), OnBackgroundChanged));

        public static SolidColorBrush GetBackground(DependencyObject obj) => (SolidColorBrush)obj.GetValue(BackgroundProperty);
        public static void SetBackground(DependencyObject obj, SolidColorBrush value) => obj.SetValue(BackgroundProperty, value);

        /// <summary>
        /// 键盘文字颜色
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty= DependencyProperty.RegisterAttached(
            "Foreground",
            typeof(SolidColorBrush),
            typeof(SoftKeyboardAssist),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xff,0xff,0xff)), OnForegroundChanged));

        public static SolidColorBrush GetForeground(DependencyObject obj) => (SolidColorBrush)obj.GetValue(ForegroundProperty);
        public static void SetForeground(DependencyObject obj, SolidColorBrush value) => obj.SetValue(ForegroundProperty, value);

        /// <summary>
        /// 文本框边框颜色
        /// </summary>
        public static readonly DependencyProperty TextBorderBrushProperty= DependencyProperty.RegisterAttached(
            "TextBorderBrush",
            typeof(SolidColorBrush),
            typeof(SoftKeyboardAssist),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x62,0x69,0x73)), OnTextBorderBrushChanged));

        public static SolidColorBrush GetTextBorderBrush(DependencyObject obj) => (SolidColorBrush)obj.GetValue(TextBorderBrushProperty);
        public static void SetTextBorderBrush(DependencyObject obj, SolidColorBrush value) => obj.SetValue(TextBorderBrushProperty, value);

        /// <summary>
        /// 文本框文字颜色
        /// </summary>
        public static readonly DependencyProperty TextForegroundProperty= DependencyProperty.RegisterAttached(
            "TextForeground",
            typeof(SolidColorBrush),
            typeof(SoftKeyboardAssist),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x4A,0xc2,0xe4)), OnTextForegroundChanged));

        public static SolidColorBrush GetTextForeground(DependencyObject obj) => (SolidColorBrush)obj.GetValue(TextForegroundProperty);
        public static void SetTextForeground(DependencyObject obj, SolidColorBrush value) => obj.SetValue(TextForegroundProperty, value);

        /// <summary>
        /// 键盘按钮背景色
        /// </summary>
        public static readonly DependencyProperty KeyboardBackgroundProperty= DependencyProperty.RegisterAttached(
            "KeyboardBackground",
            typeof(SolidColorBrush),
            typeof(SoftKeyboardAssist),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x21,0x28,0x2f)), OnKeyboardBackgroundChanged));

        public static SolidColorBrush GetKeyboardBackground(DependencyObject obj) => (SolidColorBrush)obj.GetValue(KeyboardBackgroundProperty);
        public static void SetKeyboardBackground(DependencyObject obj, SolidColorBrush value) => obj.SetValue(KeyboardBackgroundProperty, value);

        private static readonly DependencyProperty PopupProperty = DependencyProperty.RegisterAttached(
            "Popup",
            typeof(Popup),
            typeof(SoftKeyboardAssist),
            new PropertyMetadata(null));

        private static Popup GetPopupProperty(DependencyObject obj) => (Popup)obj.GetValue(PopupProperty);

        private static void SetPopupProperty(DependencyObject obj, Popup value) => obj.SetValue(PopupProperty, value);

        private static readonly DependencyProperty KeyboardProperty = DependencyProperty.RegisterAttached(
            "Keyboard",
            typeof(NumericSoftKeyboard),
            typeof(SoftKeyboardAssist),
            new PropertyMetadata(null));

        private static NumericSoftKeyboard GetKeyboardProperty(DependencyObject obj) => (NumericSoftKeyboard)obj.GetValue(KeyboardProperty);

        private static void SetKeyboardProperty(DependencyObject obj, NumericSoftKeyboard value) => obj.SetValue(KeyboardProperty, value);

        private static readonly DependencyProperty JitterProperty = DependencyProperty.RegisterAttached(
    "Jitter",
    typeof(DebounceHelper),
    typeof(SoftKeyboardAssist),
    new PropertyMetadata(null));

        private static DebounceHelper GetJitterProperty(DependencyObject obj) => (DebounceHelper)obj.GetValue(JitterProperty);

        private static void SetJitterProperty(DependencyObject obj, DebounceHelper value) => obj.SetValue(JitterProperty, value);

        private static void OnUseNumericSoftKeyboardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control && e.NewValue is bool useSoftKeyboard)
            {
                if (useSoftKeyboard)
                {
                    AttachSoftKeyboard(control);
                }
                else
                {
                    DetachSoftKeyboard(control);
                }
            }
        }

        private static void AttachSoftKeyboard(Control control)
        {
            var popup = new Popup()
            {
                AllowsTransparency = true,
                Placement = PlacementMode.Relative,
                StaysOpen = false,
                PopupAnimation = PopupAnimation.Slide,
            };

            var keyboard = new NumericSoftKeyboard();
            keyboard.Mode = GetMode(control);
            keyboard.Background = GetBackground(control);
            keyboard.Foreground = GetForeground(control);
            var binding = new Binding()
            {
                Source = control,
                Path = GetDisplayValuePath(control) != null ? new PropertyPath(GetDisplayValuePath(control)) : new PropertyPath(GetValuePropertyPath(control)),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            keyboard.SetBinding(NumericSoftKeyboard.InputValueProperty, binding);
            keyboard.CancelEvent += () => popup.IsOpen = false;
            popup.Child = keyboard;

            //绑定到控件
            SetPopupProperty(control, popup);
            control.PreviewMouseLeftButtonDown += Control_PreviewMouseUp;
            SetKeyboardProperty(control, keyboard);

            // 防抖处理
            var jitter = new DebounceHelper(TimeSpan.FromMilliseconds(300));
            SetJitterProperty(control, jitter);

        }

        private static string GetValuePropertyPath(Control control)
        {
            return control switch
            {
                TextBox textBox => nameof(TextBox.Text),
                PasswordBox passwordBox => nameof(PasswordBox.Password),
                ComboBox comboBox => nameof(ComboBox.Text),
                DatePicker datePicker => nameof(DatePicker.Text),
                RichTextBox richTextBox => nameof(RichTextBox.Document),
                _ => "Text"
            };
        }

        private static void DetachSoftKeyboard(Control control)
        {
            control.PreviewMouseUp -= Control_PreviewMouseUp;

            var popup = GetPopupProperty(control);
            if (popup != null)
            {
                popup.Child = null;
                popup.IsOpen = false;
                SetPopupProperty(control, null);
            }
        }

        private static void Control_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Control control && GetUseSoftKeyboard(control))
            {
                var popup = GetPopupProperty(control);
                if (popup != null)
                {
                    popup.PlacementTarget = control;
                    // 延迟关闭，避免立即关闭导致无法点击键盘
                    Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    {
                        popup.IsOpen = true;
                        popup.Focus();

                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
            }
        }

        private static void Control_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Control control && GetUseSoftKeyboard(control))
            {
                var popup = GetPopupProperty(control);
                if (popup != null)
                {
                    // 延迟关闭，避免立即关闭导致无法点击键盘
                    Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    {
                        if (!control.IsFocused)
                        {
                            popup.IsOpen = false;
                        }
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
            }
        }

        private static void Control_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // 通过鼠标点击触发键盘显示（用于TextBox等控件）
            if (sender is Control control && GetUseSoftKeyboard(control))
            {
                var jitter = GetJitterProperty(control);
                if (jitter == null || !jitter.CanProceed())
                    return;

                var popup = GetPopupProperty(control);
                if (popup != null)
                {
                    var window = Application.Current.MainWindow;
                    popup.PlacementTarget = window;
                    popup.PlacementRectangle = new Rect(window.ActualWidth - 360, 120, 0, 0);
                    Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    {
                        popup.IsOpen = true;
                        popup.Focus();

                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
            }

            e.Handled = true;
        }


        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control && GetUseSoftKeyboard(control))
            {
                var _keyboard = GetKeyboardProperty(control);
                if (_keyboard != null)
                {
                    _keyboard.Mode = (SoftKeyboardMode)e.NewValue;
                }
            }
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control && GetUseSoftKeyboard(control))
            {
                var _keyboard = GetKeyboardProperty(control);
                if (_keyboard != null)
                {
                    _keyboard.Title = (string)e.NewValue;
                }
            }
        }

        private static void OnUpKeyCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control && GetUseSoftKeyboard(control))
            {
                var _keyboard = GetKeyboardProperty(control);
                if (_keyboard != null)
                {
                    _keyboard.UpKeyCommand = (ICommand)e.NewValue;
                }
            }
        }

        private static void OnDownKeyCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control && GetUseSoftKeyboard(control))
            {
                var _keyboard = GetKeyboardProperty(control);
                if (_keyboard != null)
                {
                    _keyboard.DownKeyCommand = (ICommand)e.NewValue;
                }
            }
        }

        private static void OnUnitsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control && GetUseSoftKeyboard(control))
            {
                var _keyboard = GetKeyboardProperty(control);
                if (_keyboard != null)
                {
                    _keyboard.Units = (IList)e.NewValue;
                }
            }
        }

        private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control && GetUseSoftKeyboard(control))
            {
                var _keyboard = GetKeyboardProperty(control);
                if (_keyboard != null)
                {
                    _keyboard.Step = (double)e.NewValue;
                }
            }
        }


        private static void OnDisplayValuePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control && GetUseSoftKeyboard(control))
            {
                var _keyboard = GetKeyboardProperty(control);
                if (_keyboard != null)
                {
                    var binding = new Binding()
                    {
                        Source = control,
                        Path = new PropertyPath(e.NewValue),
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    _keyboard.SetBinding(NumericSoftKeyboard.InputValueProperty, binding);
                }
            }
        }

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control && GetUseSoftKeyboard(control))
            {
                var _keyboard = GetKeyboardProperty(control);
                if (_keyboard != null)
                {
                    _keyboard.Background = (SolidColorBrush)e.NewValue;
                }
            }
        }


        private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control && GetUseSoftKeyboard(control))
            {
                var _keyboard = GetKeyboardProperty(control);
                if (_keyboard != null)
                {
                    _keyboard.Foreground = (SolidColorBrush)e.NewValue;
                }
            }
        }


        private static void OnTextBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control && GetUseSoftKeyboard(control))
            {
                var _keyboard = GetKeyboardProperty(control);
                if (_keyboard != null)
                {
                    _keyboard.TextBorderBrush = (SolidColorBrush)e.NewValue;
                }
            }
        }

        private static void OnTextForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control && GetUseSoftKeyboard(control))
            {
                var _keyboard = GetKeyboardProperty(control);
                if (_keyboard != null)
                {
                    _keyboard.TextForeground = (SolidColorBrush)e.NewValue;
                }
            }
        }

        private static void OnKeyboardBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control && GetUseSoftKeyboard(control))
            {
                var _keyboard = GetKeyboardProperty(control);
                if (_keyboard != null)
                {
                    _keyboard.KeyboardBackground = (SolidColorBrush)e.NewValue;
                }
            }
        }

    }
}
