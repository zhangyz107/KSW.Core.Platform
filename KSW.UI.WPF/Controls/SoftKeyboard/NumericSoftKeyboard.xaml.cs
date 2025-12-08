using KSW.UI.WPF.Enums;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KSW.UI.WPF.Controls
{
    /// <summary>
    /// NumericSoftKeyboard.xaml 的交互逻辑
    /// </summary>
    public partial class NumericSoftKeyboard : UserControl, INotifyPropertyChanged
    {

        #region Private
        private bool _clearAll = true;
        private string _displayValue;
        private ICommand _upKeyCommand;
        private ICommand _downKeyCommand;

        private DelegateCommand<string> _clickComamd;
        private DelegateCommand<string> _sureCommand;
        private DelegateCommand _cancelCommand;

        #endregion

        #region NotifyPropertyChanged
        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <param name="onChanged">Action that is called after the property value has been changed.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, Action? onChanged,
            [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            onChanged?.Invoke();
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="args">The PropertyChangedEventArgs</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }
        #endregion

        #region Properties
        //public event Action
        public event Action CancelEvent;
        public event PropertyChangedEventHandler? PropertyChanged;

        public string InputValue
        {
            get { return (string)GetValue(InputValueProperty); }
            set { SetValue(InputValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InputValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputValueProperty =
            DependencyProperty.Register("InputValue", typeof(string), typeof(NumericSoftKeyboard), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnInputValueChanged));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(NumericSoftKeyboard));

        public SoftKeyboardMode Mode
        {
            get { return (SoftKeyboardMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(SoftKeyboardMode), typeof(NumericSoftKeyboard), new PropertyMetadata(SoftKeyboardMode.Multiple, OnModeChanged));

        public IList Units
        {
            get { return (IList)GetValue(UnitsProperty); }
            set { SetValue(UnitsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Units.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnitsProperty =
            DependencyProperty.Register("Units", typeof(IList), typeof(NumericSoftKeyboard), new PropertyMetadata(null, OnUnitsChanged));

        public double Step
        {
            get { return (double)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Step.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register("Step", typeof(double), typeof(NumericSoftKeyboard), new PropertyMetadata(0.0));

        /// <summary>
        /// 显示值
        /// </summary>
        public string DisplayValue
        {
            get => _displayValue;
            set => SetProperty(ref _displayValue, value);
        }

        /// <summary>
        /// 上键命令
        /// </summary>
        public ICommand UpKeyCommand
        {
            get => _upKeyCommand;
            set => SetProperty(ref _upKeyCommand, value);
        }

        /// <summary>
        /// 下键命令
        /// </summary>
        public ICommand DownKeyCommand
        {
            get => _downKeyCommand;
            set => SetProperty(ref _downKeyCommand, value);
        }
        #endregion

        #region Commands
        public DelegateCommand<string> ClickComamd => _clickComamd ?? (_clickComamd = new DelegateCommand<string>(ExecuteClickCommand));

        public DelegateCommand<string> SureCommand =>
            _sureCommand ?? (_sureCommand = new DelegateCommand<string>(ExecuteSureCommand));

        public DelegateCommand CancelCommand => _cancelCommand ?? (_cancelCommand = new DelegateCommand(ExecuteCancelCommand));

        #endregion

        public NumericSoftKeyboard()
        {
            InitializeComponent();
            DataContext = this;

            this.Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitData();
        }

        private void InitData()
        {
            _clearAll = true;
            DisplayValue = InputValue;
        }

        private static void OnInputValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericSoftKeyboard keyboard)
            {
                keyboard.DisplayValue = e.NewValue.ToString();
            }
        }

        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericSoftKeyboard keyboard && e.NewValue is SoftKeyboardMode mode)
            {
                switch (mode)
                {
                    case SoftKeyboardMode.Single:
                        keyboard.MultiplePanel.Visibility = Visibility.Collapsed;
                        keyboard.SinglePanel.Visibility = Visibility.Visible;
                        break;
                    case SoftKeyboardMode.Multiple:
                        keyboard.MultiplePanel.Visibility = Visibility.Visible;
                        keyboard.SinglePanel.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        break;
                }
            }
        }


        private static void OnUnitsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericSoftKeyboard keyboard && e.NewValue is IList units)
            {
                if (units.Count > 0)
                {
                    var mode = keyboard.Mode;
                    switch (mode)
                    {
                        case SoftKeyboardMode.Single:
                            keyboard.Btn_Unit.Content = units[0]?.ToString();
                            break;
                        case SoftKeyboardMode.Multiple:
                            keyboard.Btn_Unit1.Content = units[0]?.ToString();
                            if (units.Count > 1)
                                keyboard.Btn_Unit2.Content = units[1]?.ToString();
                            if (units.Count > 2)
                                keyboard.Btn_Unit3.Content = units[2]?.ToString();
                            if (units.Count > 3)
                                keyboard.Btn_Unit4.Content = units[3]?.ToString();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void ExecuteClickCommand(string obj)
        {
            if (_clearAll)
            {
                if (obj.Equals("."))
                    return;
                else if (!InputValue.IsEmpty())
                {
                    DisplayValue = string.Empty;
                    _clearAll = false;
                }
            }

            if (int.TryParse(obj, out int _))
            {
                if (_clearAll && !InputValue.IsEmpty())
                {
                    DisplayValue = string.Empty;
                    _clearAll = false;
                }

                if (DisplayValue == "0")
                    DisplayValue = obj;
                else
                    DisplayValue += obj;
            }
            else if (obj.Equals(".") || obj.Equals("Back") || obj.Equals("Negate"))
            {
                //处理"."
                if (obj.Equals(".") && !DisplayValue.Contains("."))
                {
                    DisplayValue += ".";
                    return;
                }

                //处理"Back"
                if (obj.Equals("Back") && DisplayValue.Length > 0)
                {
                    DisplayValue = DisplayValue.Substring(0, DisplayValue.Length - 1);
                    return;
                }

                //处理"Negate"
                if (obj.Equals("Negate") && DisplayValue.Length > 0)
                {
                    if (DisplayValue.Contains("-"))
                        DisplayValue = DisplayValue.TrimStart('-');
                    else
                        DisplayValue = "-" + DisplayValue;
                }
            }
        }

        private void ExecuteSureCommand(string obj)
        {
            if (DisplayValue.IsEmpty())
            {
                CancelEvent?.Invoke();
                return;
            }

            var stepValue = 1.0;
            if (Step != 0 && int.TryParse(obj, out int stepRatio))
                stepValue = Math.Pow(Step, stepRatio);

            if (double.TryParse(DisplayValue, out double value))
            {
                value *= stepValue;
                InputValue = value.ToString();
            }

            CancelEvent?.Invoke();
        }

        private void ExecuteCancelCommand()
        {
            CancelEvent?.Invoke();
        }
    }
}
