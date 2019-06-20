using System;
using System.Globalization;
using System.Windows.Data;

namespace HardwareSimulator.Converters
{
    public class DebugConverter : IMultiValueConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => values;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => new object[] { value };
    }
}
