using System;
using System.Globalization;
using System.Windows.Data;

namespace HardwareSimulator.Converters
{
    public class TupleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => parameter is string s && int.TryParse(s, out var i) && value is System.Runtime.CompilerServices.ITuple tuple ? tuple[i] : throw new ArgumentOutOfRangeException(nameof(parameter));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
