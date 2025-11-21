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
    [TemplatePart(Name = PART_Header,Type = typeof(ContentPresenter))]
    [TemplatePart(Name = PART_Icon, Type = typeof(Panel))]
    [TemplatePart(Name = PART_Content, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = PART_Time, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_RootGrid, Type = typeof(Grid))]
    public class TimelineItem : HeaderedContentControl
    {
        public const string PART_Header = "PART_Header";
        public const string PART_Icon = "PART_Icon";
        public const string PART_Content = "PART_Content";
        public const string PART_Time = "PART_Time";
        public const string PART_RootGrid = "PART_RootGrid";

        private ContentPresenter? _headerPresenter;
        private Panel? _iconPresenter;
        private ContentPresenter? _contentPresenter;
        private TextBlock? _timePresenter;
        private Grid? _rootGrid;

        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(object), typeof(TimelineItem));



        public DataTemplate IconTemplate
        {
            get { return (DataTemplate)GetValue(IconTemplateProperty); }
            set { SetValue(IconTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconTemplateProperty =
            DependencyProperty.Register("IconTemplate", typeof(DataTemplate), typeof(TimelineItem));



        public DataTemplateSelector IconTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(IconTemplateSelectorProperty); }
            set { SetValue(IconTemplateSelectorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconTemplateSelectorProperty =
            DependencyProperty.Register("IconTemplateSelector", typeof(DataTemplateSelector), typeof(TimelineItem));

        public TimelineItemType Type
        {
            get { return (TimelineItemType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Type.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(TimelineItemType), typeof(TimelineItem));

        public TimelineItemPosition Position
        {
            get { return (TimelineItemPosition)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(TimelineItemPosition), typeof(TimelineItem));

        public DateTime Time
        {
            get { return (DateTime)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Time.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(DateTime), typeof(TimelineItem));

        public string TimeFormat
        {
            get { return (string)GetValue(TimeFormatProperty); }
            set { SetValue(TimeFormatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimeFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeFormatProperty =
            DependencyProperty.Register("TimeFormat", typeof(string), typeof(TimelineItem));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _rootGrid = GetTemplateChild(PART_RootGrid) as Grid; 
            _headerPresenter = GetTemplateChild(PART_Header) as ContentPresenter;
            _iconPresenter = GetTemplateChild(PART_Icon) as Panel;
            _contentPresenter = GetTemplateChild(PART_Content) as ContentPresenter;
            _timePresenter = GetTemplateChild(PART_Time) as TextBlock;
        }

        internal (double left, double mid, double right) GetWidth()
        {
            if (_headerPresenter == null) return (0, 0, 0);

            double header = _headerPresenter.DesiredSize.Width;
            double icon = _iconPresenter?.DesiredSize.Width ?? 0;
            double content = _contentPresenter?.DesiredSize.Width ?? 0;
            double time = _timePresenter?.DesiredSize.Width ?? 0;
            double max = Math.Max(header, content);

            if (Position == TimelineItemPosition.Left)
            {
                max = Math.Max(max, time);
                return (0, icon, max);
            }
            if (Position == TimelineItemPosition.Right)
            {
                max = Math.Max(max, time);
                return (max, icon, 0);
            }
            if (Position == TimelineItemPosition.Separate)
            {
                return (time, icon, max);
            }
            return (0, 0, 0);
        }

        internal void SetWidth(double? left, double? mid, double? right)
        {
            if (_rootGrid is null) return;
            _rootGrid.ColumnDefinitions[0].Width = new GridLength(left ?? 0);
            _rootGrid.ColumnDefinitions[1].Width = new GridLength(mid ?? 0);
            _rootGrid.ColumnDefinitions[2].Width = new GridLength(right ?? 0);
        }

        internal void SetIfUnset<T>(DependencyProperty property, T value)
        {
            if (!IsSet(property))
            {
                SetCurrentValue(property, value);
            }
        }
        public bool IsSet(DependencyProperty property)
        {
            property = property ?? throw new ArgumentNullException(nameof(property));

            VerifyAccess();

            return ReadLocalValue(property) != DependencyProperty.UnsetValue;
        }
    }
}
