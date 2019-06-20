using System;
using System.Globalization;
using System.Windows.Data;
#if Computer8Bits
using DataValue = HardwareSimulator.Core.DataValue8Bits;
using InnerType = System.Byte;
#elif Computer16Bits
using DataValue = HardwareSimulator.Core.DataValue16Bits;
using InnerType = System.UInt16;
#endif

namespace HardwareSimulator.Converters
{
    public class DataValueToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return null;

            if(value is DataValue data && targetType == typeof(bool?))
                return (bool)data;
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
