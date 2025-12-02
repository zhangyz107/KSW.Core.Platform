using KSW.UI.WPF.Enums;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace KSW.UI.WPF.Controls
{
    public class Anchor : ItemsControl
    {
        private CancellationTokenSource _cts = new();
        private List<(string, double)> _positions = [];
        private bool _scrollingFromSelection;
        private AnchorItem _selectedContainer;

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdProperty =
            DependencyProperty.RegisterAttached("Id", typeof(string), typeof(Anchor), new PropertyMetadata(null));

        public static void SetId(DependencyObject obj, string? value)
        {
            obj.SetValue(IdProperty, value);
        }

        public static string? GetId(DependencyObject obj)
        {
            return (string?)obj.GetValue(IdProperty);
        }

        public ScrollViewer TargetContainer
        {
            get { return (ScrollViewer)GetValue(TargetContainerProperty); }
            set { SetValue(TargetContainerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetContainer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetContainerProperty =
            DependencyProperty.Register("TargetContainer", typeof(ScrollViewer), typeof(Anchor), new PropertyMetadata(null, OnTargetContainerChanged));

        public double TopOffset
        {
            get { return (double)GetValue(TopOffsetProperty); }
            set { SetValue(TopOffsetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TopOffset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TopOffsetProperty =
            DependencyProperty.Register("TopOffset", typeof(double), typeof(Anchor));

        public AnchorMode Mode
        {
            get { return (AnchorMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(AnchorMode), typeof(Anchor), new PropertyMetadata(AnchorMode.Primary, OnDisplayModeChanged));

        private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var anchor = (Anchor)d;
            anchor.OnDisplayModeChanged(e);
        }

        private void OnDisplayModeChanged(DependencyPropertyChangedEventArgs e)
        {
            SetItemMode();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is AnchorItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new AnchorItem();
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            SetItemMode();
            return base.ArrangeOverride(arrangeBounds);
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            SetDefaultItem();
        }

        private static void OnTargetContainerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var anchor = (Anchor)d;
            var oldContainer = e.OldValue as ScrollViewer;
            var newContainer = e.NewValue as ScrollViewer;

            if (oldContainer != null)
            {
                oldContainer.Loaded -= anchor.OnTargetContainerLoaded;
                oldContainer.ScrollChanged -= anchor.OnScrollChanged;
            }

            if (newContainer != null)
            {
                newContainer.Loaded += anchor.OnTargetContainerLoaded;
                newContainer.ScrollChanged += anchor.OnScrollChanged;

                if (newContainer.IsLoaded)
                    anchor.InvalidateAnchorPositions();
            }
        }

        private async void ScrollToAnchor(FrameworkElement target)
        {
            if (TargetContainer is null)
                return;

            var targetPosition = target.TransformToAncestor(TargetContainer).Transform(new Point(0, 0));
            var from = TargetContainer.VerticalOffset;
            var to = TargetContainer.VerticalOffset + targetPosition.Y - TopOffset;
            if (to > TargetContainer.ScrollableHeight)
                to = TargetContainer.ScrollableHeight;
            if (Math.Abs(from - to) < 0.1) return;

            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _scrollingFromSelection = true;
            // WPF中的动画实现
            await SmoothScrollAsync(from, to, TimeSpan.FromSeconds(0.3), token).ContinueWith(async _ => { await Task.Delay(30); _scrollingFromSelection = false; }, token); //加时延防止点击事件误触发滚轮变化事件
        }

        private async Task SmoothScrollAsync(double from, double to, TimeSpan duration, CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;

            while (!cancellationToken.IsCancellationRequested)
            {
                var elapsed = DateTime.Now - startTime;
                var progress = Math.Min(elapsed.TotalMilliseconds / duration.TotalMilliseconds, 1.0);

                // 使用缓动函数
                var easedProgress = GetQuadraticEaseOutProgress(progress);
                var currentValue = from + (to - from) * easedProgress;

                await Dispatcher.InvokeAsync(() =>
                {
                    TargetContainer.ScrollToVerticalOffset(currentValue);
                }, DispatcherPriority.Render);

                if (progress >= 1.0)
                    break;

                await Task.Delay(16, cancellationToken); // ~60fps
            }
        }

        private double GetQuadraticEaseOutProgress(double progress)
        {
            return -1 * progress * (progress - 2);
        }

        public void InvalidatePositions()
        {
            InvalidateAnchorPositions();
            MarkSelectedContainerByPosition();
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_scrollingFromSelection) return;
            MarkSelectedContainerByPosition();
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            var source = GetParentAnchorItem(e.OriginalSource as DependencyObject);
            if (source is null) return;

            MarkSelectedContainer(source);
            var target = FindVisualChildren<FrameworkElement>(TargetContainer)
    .FirstOrDefault(a => GetId(a) == source.AnchorId);

            if (target is null) return;
            ScrollToAnchor(target);
        }

        private AnchorItem GetParentAnchorItem(DependencyObject? element)
        {
            while (element != null)
            {
                if (element is AnchorItem anchorItem)
                    return anchorItem;
                element = VisualTreeHelper.GetParent(element);
            }
            return null;
        }

        private void MarkSelectedContainer(AnchorItem item)
        {
            if (item == null) return;

            var oldValue = _selectedContainer;
            var newValue = item;
            if (oldValue == newValue) return;

            _selectedContainer?.SetValue(AnchorItem.IsSelectedProperty, false);
            _selectedContainer = newValue;
            _selectedContainer?.SetValue(AnchorItem.IsSelectedProperty, true);
        }

        private void MarkSelectedContainerByPosition()
        {
            if (TargetContainer is null) return;
            var top = TargetContainer.VerticalOffset + TopOffset;
            var topAnchorId = _positions.LastOrDefault(a => a.Item2 <= top).Item1;
            if (topAnchorId is null) return;

            var item = FindVisualChildren<AnchorItem>(this).FirstOrDefault(a => a.AnchorId == topAnchorId);
            if (item is null) return;
            MarkSelectedContainer(item);
        }

        private void OnTargetContainerLoaded(object sender, RoutedEventArgs e)
        {
            InvalidateAnchorPositions();
        }

        private void InvalidateAnchorPositions()
        {
            if (TargetContainer is null) return;

            var items = FindVisualChildren<FrameworkElement>(TargetContainer)
                .Where(a => GetId(a) is not null);

            var positions = new List<(string, double)>();
            foreach (var item in items)
            {
                var anchorId = GetId(item);
                if (anchorId is null) continue;

                var position = item.TransformToAncestor(TargetContainer).Transform(new Point(0, 0)).Y + TargetContainer.VerticalOffset;
                positions.Add((anchorId, position));
            }

            positions.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            _positions = positions;
        }


        private void SetItemMode()
        {
            var panel = FindVisualChild<StackPanel>(this);

            if (panel != null)
            {
                var items = panel.Children.OfType<AnchorItem>();
                switch (Mode)
                {
                    case AnchorMode.Primary:
                        foreach (var item in items)
                            SetIfUnset(item, AnchorItem.DisplayModeProperty, AnchorMode.Primary);
                        break;
                    case AnchorMode.Small:
                        foreach (var item in items)
                            SetIfUnset(item, AnchorItem.DisplayModeProperty, AnchorMode.Small);
                        break;
                    case AnchorMode.Tertiary:
                        foreach (var item in items)
                            SetIfUnset(item, AnchorItem.DisplayModeProperty, AnchorMode.Tertiary);
                        break;
                    case AnchorMode.Muted:
                        foreach (var item in items)
                            SetIfUnset(item, AnchorItem.DisplayModeProperty, AnchorMode.Muted);
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

        // 查找可视化子元素的辅助方法
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    yield return result;

                foreach (var descendant in FindVisualChildren<T>(child))
                    yield return descendant;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            //设置第一项为默认选项
            SetDefaultItem();

            if (TargetContainer?.IsLoaded == true)
                InvalidateAnchorPositions();

            MarkSelectedContainerByPosition();
        }

        private void SetDefaultItem()
        {
            if (HasItems)
            {
                var anchorItem = Items[0] as AnchorItem;
                anchorItem.IsSelected = true;
            }
        }

        internal FrameworkElement GetContainerForItemOverrideInternal()
        {
            return new AnchorItem();
        }

        internal bool NeedsContainerOverrideInternal(object? item, int index, out object? recycleKey)
        {
            recycleKey = null;
            return !IsItemItsOwnContainerOverrideInternal(item);
        }

        internal bool IsItemItsOwnContainerOverrideInternal(object item)
        {
            return item is AnchorItem;
        }

        internal void PrepareContainerForItemOverrideInternal(FrameworkElement container, object? item, int index)
        {
            if (container is DependencyObject depObj)
            {
                // 调用基类的PrepareContainerForItemOverride
                PrepareContainerForItemOverride(depObj, item);

                // 额外的准备逻辑
                if (container is AnchorItem anchorItem)
                {
                    // 设置AnchorItem的特定属性
                }
            }
        }

        internal void ContainerForItemPreparedOverrideInternal(FrameworkElement container, object? item, int index)
        {

        }

        private void SetIfUnset(Control target, DependencyProperty property, object value)
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
