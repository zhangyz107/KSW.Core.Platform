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

        public static SoftKeyboardMode GetMode(DependencyObject obj)
        {
            return (SoftKeyboardMode)obj.GetValue(ModeProperty);
        }

        public static void SetMode(DependencyObject obj, SoftKeyboardMode value)
        {
            obj.SetValue(ModeProperty, value);
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.RegisterAttached(
            "Title",
            typeof(string),
            typeof(SoftKeyboardAssist),
            new PropertyMetadata(null, OnTitleChanged));

        public static string GetTitle(DependencyObject obj) => (string)obj.GetValue(TitleProperty);

        public static void SetTitle(DependencyObject obj, string value) => obj.SetValue(TitleProperty, value);

        public static readonly DependencyProperty UpKeyCommandProperty = DependencyProperty.RegisterAttached(
        "UpKeyCommand",
        typeof(ICommand),
        typeof(SoftKeyboardAssist),
        new PropertyMetadata(null, OnUpKeyCommandChanged));

        public static ICommand GetUpKeyCommand(DependencyObject obj) => (ICommand)obj.GetValue(UpKeyCommandProperty);

        public static void SetUpKeyCommand(DependencyObject obj, ICommand value) => obj.SetValue(UpKeyCommandProperty, value);

        public static readonly DependencyProperty DownKeyCommandProperty = DependencyProperty.RegisterAttached(
        "DownKeyCommand",
        typeof(ICommand),
        typeof(SoftKeyboardAssist),
        new PropertyMetadata(null, OnDownKeyCommandChanged));

        public static ICommand GetDownKeyCommand(DependencyObject obj) => (ICommand)obj.GetValue(DownKeyCommandProperty);

        public static void SetDownKeyCommand(DependencyObject obj, ICommand value) => obj.SetValue(DownKeyCommandProperty, value);

        public static readonly DependencyProperty UnitsProperty = DependencyProperty.RegisterAttached(
        "Units",
        typeof(IList),
        typeof(SoftKeyboardAssist),
        new PropertyMetadata(null, OnUnitsChanged));

        public static IList GetUnits(DependencyObject obj) => (IList)obj.GetValue(UnitsProperty);

        public static void SetUnits(DependencyObject obj, IList value) => obj.SetValue(UnitsProperty, value);

        public static readonly DependencyProperty StepProperty= DependencyProperty.RegisterAttached(
            "Step",
            typeof(double),
            typeof(SoftKeyboardAssist),
            new PropertyMetadata(0.0,OnStepChanged));

        public static double GetStep(DependencyObject obj) => (double)obj.GetValue(StepProperty);

        public static void SetStep(DependencyObject obj, double value) => obj.SetValue(StepProperty, value);

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
                Placement = PlacementMode.Center,
                StaysOpen = false,
                PopupAnimation = PopupAnimation.Slide
            };

            var keyboard = new NumericSoftKeyboard();
            keyboard.Mode = GetMode(control);
            var binding = new Binding()
            {
                Source = control,
                Path = new PropertyPath(GetValuePropertyPath(control)),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            keyboard.SetBinding(NumericSoftKeyboard.InputValueProperty, binding);
            keyboard.CancelEvent += () => popup.IsOpen = false;
            popup.Child = keyboard;

            //绑定到控件
            SetPopupProperty(control, popup);
            control.PreviewMouseUp += Control_PreviewMouseUp;

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
                    popup.IsOpen = true;
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
                    popup.PlacementTarget = control;

                    // 延迟关闭，避免立即关闭导致无法点击键盘
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

    }
}
