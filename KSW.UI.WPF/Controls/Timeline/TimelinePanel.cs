using KSW.UI.WPF.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KSW.UI.WPF.Controls
{
    public class TimelinePanel : Panel
    {
        public TimelineDisplayMode Mode
        {
            get { return (TimelineDisplayMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(int), typeof(TimelinePanel));

        protected override Size MeasureOverride(Size availableSize)
        {
            double left = 0;
            double right = 0;
            double icon = 0;
            double height = 0;

            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);
                if (child is TimelineItem t)
                {
                    var doubles = t.GetWidth();
                    left = Math.Max(left, doubles.left);
                    icon = Math.Max(icon, doubles.mid);
                    right = Math.Max(right, doubles.right);
                }
                height += child.DesiredSize.Height;
            }

            return new Size(left + icon + right, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double left = 0, mid = 0, right = 0;
            double height = 0;
            foreach (UIElement child in Children)
            {
                if (child is TimelineItem t)
                {
                    var doubles = t.GetWidth();
                    left = Math.Max(left, doubles.left);
                    mid = Math.Max(mid, doubles.mid);
                    right = Math.Max(right, doubles.right);
                }
            }

            Rect rect = new Rect(0, 0, left + mid + right, 0);
            foreach (UIElement child in Children)
            {
                if (child is TimelineItem t)
                {
                    t.SetWidth(left, mid, right);
                    t.InvalidateArrange();
                    rect.Height = t.DesiredSize.Height;
                    child.Arrange(rect);
                    rect.Y += t.DesiredSize.Height;
                    height += t.DesiredSize.Height;
                }
            }
            return new Size(left + mid + right, height);
        }
    }
}
