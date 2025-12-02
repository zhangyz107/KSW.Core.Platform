using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KSW.UI.WPF.Controls
{
    public class ScrollViewerBehavior : DependencyObject
    {
        // Using a DependencyProperty as the backing store for VerticalOffset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.Register("VerticalOffset", typeof(double), typeof(ScrollViewerBehavior), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVerticalOffsetChanged));

        public static void SetVerticalOffset(ScrollViewer scrollViewer, double value)
        {
            scrollViewer.SetValue(VerticalOffsetProperty, value);
        }

        public static double GetVerticalOffset(ScrollViewer scrollViewer)
        {
            return (double)scrollViewer.GetValue(VerticalOffsetProperty);
        }

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }
    }
}
