using KSW.UI.WPF.Enums;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace KSW.UI.WPF.Controls
{
    [TemplatePart(Name = PART_Pipe, Type = typeof(Border))]
    [TemplatePart(Name = PART_Pipe, Type = typeof(ContentPresenter))]
    public class AnchorItem : HeaderedItemsControl
    {
        public const string PART_Pipe = "PART_Pipe";
        public const string PART_HeaderPresenter = "PART_HeaderPresenter";


        private Anchor _root;
        private static readonly ItemsPanelTemplate DefaultPanel =
    new ItemsPanelTemplate(new FrameworkElementFactory(typeof(StackPanel)));

        public string AnchorId
        {
            get { return (string)GetValue(AnchorIdProperty); }
            set { SetValue(AnchorIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AnchorId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AnchorIdProperty =
            DependencyProperty.Register("AnchorId", typeof(string), typeof(AnchorItem), new PropertyMetadata(string.Empty));

        [Bindable(true), Category("Appearance")]
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(typeof(AnchorItem), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnIsSelectedChanged));

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as AnchorItem;
            var newValue = (bool)e.NewValue;

            if (newValue)
            {
                control.ClearSelection(control);
            }

            control.OnIsSelectedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        private void ClearSelection(AnchorItem anchorItem)
        {
            if (anchorItem.Parent is Anchor parent)
            {
                foreach (var child in parent.Items)
                {
                    if (child is AnchorItem item)
                    {
                        if (item != this && item.IsSelected)
                            item.IsSelected = false;

                        if (item.HasItems)
                            ClearAllChildrenSelection(item);
                    }
                }
            }
            else if (anchorItem.Parent is AnchorItem anchorItemParent)
            {
                ClearSelection(anchorItemParent);

                foreach (var child in anchorItemParent.Items)
                {
                    if (child is AnchorItem item
                        && item != anchorItem
                        && item.IsSelected)
                    {
                        item.IsSelected = false;
                    }
                }
            }
        }

        private void ClearAllChildrenSelection(AnchorItem parent)
        {
            foreach (var item in parent.Items)
            {
                var container = parent.ItemContainerGenerator.ContainerFromItem(item) as AnchorItem;
                if (container != null && container != this && container.IsSelected)
                {
                    container.SetCurrentValue(IsSelectedProperty, false);
                    ClearAllChildrenSelection(container);
                }
            }
        }

        // 实例方法处理变化
        protected virtual void OnIsSelectedChanged(bool oldValue, bool newValue)
        {
            // 这里可以添加选中状态改变时的逻辑
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if (IsSelected)
            {
                // 选中状态的视觉处理
                VisualStateManager.GoToState(this, "Selected", true);
            }
            else
            {
                // 未选中状态的视觉处理
                VisualStateManager.GoToState(this, "Normal", true);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (!this.IsSelected)
                this.IsSelected = true;
            e.Handled = true;
        }

        public int Level
        {
            get { return (int)GetValue(LevelProperty); }
            set { SetValue(LevelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Level.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LevelProperty =
            DependencyProperty.Register("Level", typeof(int), typeof(AnchorItem));

        public AnchorMode DisplayMode
        {
            get { return (AnchorMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof(AnchorMode), typeof(AnchorItem), new PropertyMetadata(AnchorMode.Primary));

        static AnchorItem()
        {
            ItemsPanelProperty.OverrideMetadata(typeof(AnchorItem), new FrameworkPropertyMetadata(DefaultPanel));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnchorItem), new FrameworkPropertyMetadata(typeof(AnchorItem)));
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            _root = FindParentAnchor(this);
            if (_root != null)
            {
                Level = CalculateDistanceFromParent<Anchor, AnchorItem>(this);

                // 继承根节点的模板设置
                if (ItemTemplate == null && _root.ItemTemplate != null)
                    SetValue(ItemTemplateProperty, _root.ItemTemplate);

                // 使用ItemContainerStyle来模拟ItemContainerTheme
                if (ItemContainerStyle == null && _root.ItemContainerStyle != null)
                    SetValue(ItemContainerStyleProperty, _root.ItemContainerStyle);
            }
        }

        private static Anchor FindParentAnchor(DependencyObject child)
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is Anchor anchor)
                    return anchor;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private static int CalculateDistanceFromParent<T, TItem>(TItem item, int defaultValue = -1)
       where T : class
       where TItem : DependencyObject
        {
            if (item == null)
                return defaultValue;

            var result = 0;
            DependencyObject? current = item;

            while (current != null && !(current is T))
            {
                if (current is TItem)
                    result++;

                current = VisualTreeHelper.GetParent(current);
            }

            return result;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return EnsureRoot().IsItemItsOwnContainerOverrideInternal(item);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return EnsureRoot().GetContainerForItemOverrideInternal();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (element is FrameworkElement container)
            {
                int index = ItemContainerGenerator.IndexFromContainer(element);
                EnsureRoot().PrepareContainerForItemOverrideInternal(container, item, index);
            }
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            // 模拟ContainerForItemPreparedOverride
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var newItem in e.NewItems)
                {
                    var container = ItemContainerGenerator.ContainerFromItem(newItem);
                    if (container is FrameworkElement uiElement)
                    {
                        int index = ItemContainerGenerator.IndexFromContainer(container);
                        EnsureRoot().ContainerForItemPreparedOverrideInternal(uiElement, newItem, index);
                    }
                }
            }
        }

        private Anchor EnsureRoot()
        {
            return _root ?? throw new InvalidOperationException("AnchorItem must be inside an Anchor control.");
        }
    }
}
