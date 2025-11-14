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
    public class VisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = Visibility.Collapsed;
            if (value is Visibility visibility)
            {
                switch (visibility)
                {
                    case Visibility.Visible:
                        result = Visibility.Collapsed;
                        break;
                    case Visibility.Hidden:
                    case Visibility.Collapsed:
                        result = Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
