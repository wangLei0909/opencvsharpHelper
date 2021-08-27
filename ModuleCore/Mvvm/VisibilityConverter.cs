using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ModuleCore.Mvvm
{
    public class VisibilityConverter : IValueConverter
    {
        //图标名转图标
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var display = (bool)value;
                if (display)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}