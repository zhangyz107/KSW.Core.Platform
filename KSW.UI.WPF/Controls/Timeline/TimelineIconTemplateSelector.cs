using KSW.UI.WPF.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KSW.UI.WPF.Controls
{
    public class TimelineIconTemplateSelector : DataTemplateSelector
    {
        public ResourceDictionary Resources { get; set; } = new ResourceDictionary();

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is TimelineItemType timelineItemType)
            {
                string key = timelineItemType.ToString();
                if (Resources.Contains(key))
                {
                    object resource = Resources[key];
                    if (resource is SolidColorBrush colorBrush)
                    {
                        // 创建DataTemplate
                        DataTemplate template = new DataTemplate();

                        // 设置可视化树
                        FrameworkElementFactory ellipseFactory = new FrameworkElementFactory(typeof(Ellipse));
                        ellipseFactory.SetValue(Ellipse.WidthProperty, 12.0);
                        ellipseFactory.SetValue(Ellipse.HeightProperty, 12.0);
                        ellipseFactory.SetValue(Ellipse.FillProperty, colorBrush);

                        template.VisualTree = ellipseFactory;
                        return template;
                    }
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
