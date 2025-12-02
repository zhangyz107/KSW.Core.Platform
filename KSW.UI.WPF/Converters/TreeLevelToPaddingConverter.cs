using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace KSW.UI.WPF.Converters
{
    public class TreeLevelToPaddingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 1 && values[0] is int level)
            {
                // 从应用程序资源或当前资源字典获取
                var value = Application.Current.TryFindResource("KSW.Anchor.Indent")
                            ?? 16.0; // 默认值

                if (value is Thickness indent)
                {
                    return new Thickness(Math.Max(level, 0) * indent.Left, indent.Top, indent.Right, indent.Bottom);
                }
            }
            return new Thickness(0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
