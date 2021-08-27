using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ModuleCore.Common.Converters
{
    internal class PackIconKindConverter : IValueConverter
    {
        //图标名转图标
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var kind = Enum.GetValues<PackIconKind>().Where(k => k.ToString() == value.ToString());

                if (kind.Any())
                    return kind.FirstOrDefault();
            }
            PackIconKind defaultkind = PackIconKind.Abc;
            return defaultkind;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}