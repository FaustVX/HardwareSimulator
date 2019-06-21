using System;
using System.Globalization;
using System.Windows.Data;
#if Computer8Bits
using DataValue = HardwareSimulator.Core.DataValue8Bits;
using InnerType = System.Byte;
using SignedInnerType = System.SByte;
#elif Computer16Bits
using DataValue = HardwareSimulator.Core.DataValue16Bits;
using InnerType = System.UInt16;
using SignedInnerType = System.Int16;
#endif

namespace HardwareSimulator.Converters
{
    public enum DataValueParameters
    {
        Binary = 2, //16 digits
        SignedDecimal = -10,
        Decimal = 10,
        Hexadecimal = 16, // 4 digits
    }

    public class DataValueToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return null;

            if(value is DataValue data && (parameter is null || parameter is DataValueParameters param))
                switch (parameter)
                {
                    case DataValueParameters.Binary:
                        return System.Convert.ToString(data, (int)parameter).PadLeft(DataValue.MaxBits, '0');
                    case DataValueParameters.SignedDecimal:
                        return System.Convert.ToString((SignedInnerType)(InnerType)data, -(int)parameter);
                    case DataValueParameters.Decimal:
                        return System.Convert.ToString(data, (int)parameter);
                    case DataValueParameters.Hexadecimal:
                        return System.Convert.ToString(data, (int)parameter).PadLeft(DataValue.MaxBits / 4, '0').ToUpper();
                }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
             => throw new NotImplementedException();
    }
}
