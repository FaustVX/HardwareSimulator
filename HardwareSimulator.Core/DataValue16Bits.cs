using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using DataValue = HardwareSimulator.Core.DataValue16Bits;
using InnerDataValue = HardwareSimulator.Core.DataValue8Bits;
using InnerValue = System.UInt16;
using InnerInnerValue = System.Byte;

namespace HardwareSimulator.Core
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct DataValue16Bits : IEquatable<DataValue>, IEnumerable<(int pos, InnerValue pow, bool value)>
    {
        public const int MaxBits = 16;

        [FieldOffset(0)]
        public readonly InnerValue Value;

        [FieldOffset(0)]
        public readonly InnerDataValue Lower;

        [FieldOffset(1)]
        public readonly InnerDataValue Upper;

        public bool GetAt(int bitPos)
            => ((Value >> bitPos) & 0b1) == 0b1;

        public DataValue Splice(int lenght)
            => Splice(0, lenght);

        public DataValue Splice(int start, int end, bool keepPos = false)
        {
            var value = Value;
            value <<= MaxBits - 1 - end;
            value >>= MaxBits - 1 - end;
            value >>= start;
            if (keepPos)
                value <<= start;
            return value;
        }

        public static DataValue SetAt(in DataValue data, int bitPos, bool value)
            => (InnerValue)unchecked(value ? (data.Value | (InnerValue)(0b1 << bitPos)) : (data.Value & (InnerValue) ~(0b1 << bitPos)));

        public DataValue16Bits(InnerValue value)
            : this()
        {
            Value = value;
        }

        public DataValue16Bits(InnerInnerValue upper, InnerInnerValue lower)
            : this()
        {
            Lower = lower;
            Upper = upper;
        }

        public bool Equals(DataValue other)
            => Value == other.Value;

        public override bool Equals(object obj)
            => obj is DataValue other && Equals(other);

        public override int GetHashCode()
            => Value.GetHashCode();

        public IEnumerator<(int pos, InnerValue pow, bool value)> GetEnumerator()
            => Enumerable.Range(0, MaxBits).Select(EnumaratorItem).Reverse().GetEnumerator();

        private (int, InnerValue, bool) EnumaratorItem(int i)
            => (i, (InnerValue)Math.Pow(2, i), GetAt(i));

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public static implicit operator DataValue(InnerValue value)
            => new DataValue(value);

        public static implicit operator DataValue(bool value)
            => value ? new DataValue(InnerValue.MaxValue) : new DataValue(InnerValue.MinValue);

        public static implicit operator bool(DataValue value)
            => value.Value != 0;

        public static implicit operator InnerValue(DataValue value)
            => value.Value;

        public static bool operator ==(DataValue value1, DataValue value2)
            => value1.Equals(value2);

        public static bool operator !=(DataValue value1, DataValue value2)
            => !value1.Equals(value2);

        public static DataValue operator |(DataValue value1, DataValue value2)
            => (InnerValue)(value1.Value | value2.Value);
    }
}
