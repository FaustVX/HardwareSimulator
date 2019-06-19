using HardwareSimulator.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace HardwareSimulator.Converters
{
    public class DataValueToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return null;

            if(value is DataValue data && targetType == typeof(bool?))
                switch (parameter)
                {
                    case nameof(DataValue.UpperBool):
                        return data.UpperBool;
                    case nameof(DataValue.LowerBool):
                        return data.LowerBool;
                    case null:
                        return (bool)data;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parameter));
                }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
