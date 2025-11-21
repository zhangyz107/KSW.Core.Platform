using KSW.UI.WPF.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace KSW.UI.WPF.Controls
{
    public class Timeline : ItemsControl
    {
        private static readonly ItemsPanelTemplate DefaultPanel =
    new ItemsPanelTemplate(new FrameworkElementFactory(typeof(TimelinePanel)));


        public string IconMemberBinding
        {
            get { return (string)GetValue(IconMemberBindingProperty); }
            set { SetValue(IconMemberBindingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconMemberBinding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconMemberBindingProperty =
            DependencyProperty.Register("IconMemberBinding", typeof(string), typeof(Timeline));

        public DataTemplateSelector IconTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(IconTemplateSelectorProperty); }
            set { SetValue(IconTemplateSelectorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconTemplateSelectorProperty =
            DependencyProperty.Register("IconTemplateSelector", typeof(DataTemplateSelector), typeof(Timeline));

        public DataTemplate DescriptionTemplate
        {
            get { return (DataTemplate)GetValue(DescriptionTemplateProperty); }
            set { SetValue(DescriptionTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DescriptionTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionTemplateProperty =
            DependencyProperty.Register("DescriptionTemplate", typeof(DataTemplate), typeof(Timeline));

        public string HeaderMemberBinding
        {
            get { return (string)GetValue(HeaderMemberBindingProperty); }
            set { SetValue(HeaderMemberBindingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderMemberBinding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderMemberBindingProperty =
            DependencyProperty.Register("HeaderMemberBinding", typeof(string), typeof(Timeline));

        public string ContentMemberBinding
        {
            get { return (string)GetValue(ContentMemberBindingProperty); }
            set { SetValue(ContentMemberBindingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContentMemberBinding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentMemberBindingProperty =
            DependencyProperty.Register("ContentMemberBinding", typeof(string), typeof(Timeline));

        public string TimeMemberBinding
        {
            get { return (string)GetValue(TimeMemberBindingProperty); }
            set { SetValue(TimeMemberBindingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimeMemberBinding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeMemberBindingProperty =
            DependencyProperty.Register("TimeMemberBinding", typeof(string), typeof(Timeline));

        public string TimeFormat
        {
            get { return (string)GetValue(TimeFormatProperty); }
            set { SetValue(TimeFormatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimeFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeFormatProperty =
            DependencyProperty.Register("TimeFormat", typeof(string), typeof(Timeline),new PropertyMetadata("yyyy-MM-dd HH:mm:ss"));

        public TimelineDisplayMode Mode
        {
            get { return (TimelineDisplayMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(TimelineDisplayMode), typeof(Timeline),new PropertyMetadata(TimelineDisplayMode.Left, OnDisplayModeChanged));

        private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timeline = (Timeline)d;
            timeline.OnDisplayModeChanged(e);
        }

        private void OnDisplayModeChanged(DependencyPropertyChangedEventArgs e)
        {
            var panel = GetValue(ItemsPanelProperty) as TimelinePanel;
            if (panel != null)
            {
                panel.Mode = (TimelineDisplayMode)e.NewValue;
                SetItemMode();
            }
        }

        static Timeline()
        {
            ItemsPanelProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata(DefaultPanel));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata(typeof(Timeline)));
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TimelineItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TimelineItem();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {

            base.PrepareContainerForItemOverride(element, item);

            if (element is TimelineItem itemContainer)
            {
                if (IconMemberBinding != null)
                    itemContainer.SetBinding(TimelineItem.IconProperty, IconMemberBinding);

                if (HeaderMemberBinding != null)
                    itemContainer.SetBinding(HeaderedContentControl.HeaderProperty, HeaderMemberBinding);

                if (ContentMemberBinding != null)
                    itemContainer.SetBinding(ContentControl.ContentProperty, ContentMemberBinding);

                if (TimeMemberBinding != null)
                    itemContainer.SetBinding(TimelineItem.TimeProperty, TimeMemberBinding);


                itemContainer.SetIfUnset(TimelineItem.TimeFormatProperty, TimeFormat);
                itemContainer.SetIfUnset(TimelineItem.IconTemplateSelectorProperty, IconTemplateSelector);
                itemContainer.SetIfUnset(HeaderedContentControl.HeaderTemplateProperty, ItemTemplate);
                itemContainer.SetIfUnset(ContentControl.ContentTemplateProperty, DescriptionTemplate);
            }
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var panel = GetValue(ItemsPanelProperty) as TimelinePanel;
            if (panel != null)
                panel.Mode = Mode;
            SetItemMode();
            return base.ArrangeOverride(arrangeBounds);
        }

        private void SetItemMode()
        {
            var panel = FindVisualChild<TimelinePanel>(this);

            if (panel != null)
            {
                var items = panel.Children.OfType<TimelineItem>();
                switch (Mode)
                {
                    case TimelineDisplayMode.Left:
                        foreach (var item in items)
                            SetIfUnset(item, TimelineItem.PositionProperty, TimelineItemPosition.Left);
                        break;
                    case TimelineDisplayMode.Center:
                        foreach (var item in items)
                            SetIfUnset(item, TimelineItem.PositionProperty, TimelineItemPosition.Separate);
                        break;
                    case TimelineDisplayMode.Right:
                        foreach (var item in items)
                            SetIfUnset(item, TimelineItem.PositionProperty, TimelineItemPosition.Right);
                        break;
                    case TimelineDisplayMode.Alternate:
                        var left = false;
                        foreach (var item in items)
                        {
                            SetIfUnset(item, TimelineItem.PositionProperty,
                                left ? TimelineItemPosition.Left : TimelineItemPosition.Right);
                            left = !left;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }

        private void SetIfUnset(ContentControl target, DependencyProperty property, object value)
        {
            if (!IsSet(property))
                target.SetCurrentValue(property, value);
        }

        public bool IsSet(DependencyProperty property)
        {
            property = property ?? throw new ArgumentNullException(nameof(property));

            VerifyAccess();

            return ReadLocalValue(property) != DependencyProperty.UnsetValue;
        }
    }
}
