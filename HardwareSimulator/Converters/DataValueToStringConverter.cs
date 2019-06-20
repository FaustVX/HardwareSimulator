using HardwareSimulator.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace HardwareSimulator.Converters
{
    public enum DataValueParameters
    {
        Binary = 2, //16 digits
        Octal = 8, // 6 digits
        Decimal = 10, // 6 digits
        Hexadecimal = 16, // 4 digits
    }

    public class DataValueToStringConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return null;

            if(value is DataValue data && (parameter is null || parameter is DataValueParameters param))
                switch (parameter)
                {
                    case DataValueParameters.Binary:
                        return System.Convert.ToString(data, (int)parameter).PadLeft(16, '0');
                    case DataValueParameters.Octal:
                        return System.Convert.ToString(data, (int)parameter).PadLeft(6, '0');
                    case DataValueParameters.Decimal:
                        return System.Convert.ToString(data, (int)parameter);
                    case DataValueParameters.Hexadecimal:
                        return System.Convert.ToString(data, (int)parameter).PadLeft(4, '0').ToUpper();
                }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is string s && ushort.TryParse(s, out var number) ? new DataValue(number) : new DataValue?();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
             => values[0] + " | " + values[1];

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
             => throw new NotImplementedException();
    }
}
