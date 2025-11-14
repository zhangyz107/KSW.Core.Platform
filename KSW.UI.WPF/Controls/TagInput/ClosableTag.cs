using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KSW.UI.WPF.Controls
{
    public class ClosableTag : ContentControl
    {
        public static readonly DependencyProperty IsCloseProperty =
            DependencyProperty.Register("IsClose", typeof(bool), typeof(ClosableTag), new PropertyMetadata(true));

        public static readonly RoutedEvent CloseEvent =
            EventManager.RegisterRoutedEvent("Close", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ClosableTag));

        static ClosableTag()
        {
            CloseCommand = new RoutedCommand("Close", typeof(ClosableTag));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ClosableTag), new FrameworkPropertyMetadata(typeof(ClosableTag)));
            CommandManager.RegisterClassCommandBinding(typeof(ClosableTag), new CommandBinding(CloseCommand, OnCloseExecuted));
        }

        public bool IsClose
        {
            get => (bool)GetValue(IsCloseProperty);
            set => SetValue(IsCloseProperty, value);
        }

        public static RoutedCommand CloseCommand { get; }

        public event RoutedEventHandler Close
        {
            add => AddHandler(CloseEvent, value);
            remove => RemoveHandler(CloseEvent, value);
        }

        private static void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is ClosableTag tag)
                tag.OnClose();
        }

        protected virtual void OnClose()
        {
            var args = new RoutedEventArgs(CloseEvent, this);
            RaiseEvent(args);
        }


    }
}
