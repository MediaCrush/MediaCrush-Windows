using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MediaCrush.Converters
{
    public class FileStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var test = parameter.ToString();
            if (test.StartsWith("!"))
            {
                if (value.ToString() == test.Substring(1))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
            if (value.ToString() == parameter.ToString())
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
