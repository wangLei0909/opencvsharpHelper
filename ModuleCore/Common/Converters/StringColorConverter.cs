using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ModuleCore.Common.Converters
{
    public class StringColorConverter : IValueConverter
    {
        //图标名转图标
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                string result = value.ToString();
                if (result == "OK") return new SolidColorBrush(Colors.Green);
                if (result == "NG") return new SolidColorBrush(Colors.Red);
            }
            return new SolidColorBrush(Colors.SlateGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}