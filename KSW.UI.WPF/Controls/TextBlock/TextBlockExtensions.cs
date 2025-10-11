using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KSW.UI.WPF.Controls
{
    public static class TextBlockExtensions
    {

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RequiredProperty =
            DependencyProperty.RegisterAttached("Required", typeof(bool), typeof(TextBlockExtensions), new PropertyMetadata(false, OnRequiredChanged));

        private static void OnRequiredChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        public static bool GetRequired(DependencyObject obj)
        {
            return (bool)obj.GetValue(RequiredProperty);
        }

        public static void SetRequired(DependencyObject obj)
        {
            obj.SetValue(RequiredProperty, true);
        }
    }
}
