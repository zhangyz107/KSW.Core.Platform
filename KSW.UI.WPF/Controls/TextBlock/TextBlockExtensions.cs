using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace KSW.UI.WPF.Controls
{
    public class TextBlockExtensions : DependencyObject
    {

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RequiredProperty =
            DependencyProperty.RegisterAttached("Required", typeof(bool), typeof(TextBlockExtensions), new PropertyMetadata(false, OnRequiredChanged));

        private static void OnRequiredChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                textBlock.Dispatcher.BeginInvoke(() =>
                {
                    UpdateTextBlockText(textBlock);
                });
            }
        }

        public static bool GetRequired(DependencyObject obj)
        {
            return (bool)obj.GetValue(RequiredProperty);
        }

        public static void SetRequired(DependencyObject obj, bool value)
        {
            obj.SetValue(RequiredProperty, value);
        }

        private static void UpdateTextBlockText(TextBlock textBlock)
        {
            var isRequired = GetRequired(textBlock);
            var originalText = textBlock.GetValue(TextBlock.TextProperty) as string ?? textBlock.Text;
            if (isRequired)
            { 
                var asteriskColor = GetAsteriskColor(textBlock);
                textBlock.Inlines.Clear();
                textBlock.Inlines.Add(originalText);
                textBlock.Inlines.Add(new Run("*") { Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(asteriskColor)) });
            }
            else
            {
                // 恢复原始文本
                textBlock.Inlines.Clear();
                textBlock.Inlines.Add(new Run(textBlock.Text));
            }
        }

        // 定义星号颜色附加属性
        public static readonly DependencyProperty AsteriskColorProperty =
            DependencyProperty.RegisterAttached(
                "AsteriskColor",
                typeof(string),
                typeof(TextBlockExtensions),
                new PropertyMetadata("#FF0000"));

        public static string GetAsteriskColor(DependencyObject obj)
        {
            return (string)obj.GetValue(AsteriskColorProperty);
        }

        public static void SetAsteriskColor(DependencyObject obj, string value)
        {
            obj.SetValue(AsteriskColorProperty, value);
        }
    }
}
