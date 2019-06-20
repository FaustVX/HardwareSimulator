using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace HardwareSimulator.Core
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct DataValue : IEquatable<DataValue>, IEnumerable<(int pos, int pow, bool value)>
    {
        [FieldOffset(0)]
        public readonly ushort Value;

        [FieldOffset(0)]
        public readonly bool LowerBool;

        [FieldOffset(0)]
        public readonly byte LowerByte;

        [FieldOffset(1)]
        public readonly bool UpperBool;

        [FieldOffset(1)]
        public readonly byte UpperByte;

        [FieldOffset(2)]
        public readonly byte _size;

        public bool GetAt(int bitPos)
            => ((Value >> bitPos) & 0b1) == 0b1;

        public DataValue Splice(int lenght)
            => Splice(0, lenght);

        public DataValue Splice(int start, int end, bool keepPos = false)
        {
            var value = Value;
            value <<= 15 - end;
            value >>= 15 - end;
            value >>= start;
            if (keepPos)
                value <<= start;
            return value;
        }

        public static DataValue SetAt(in DataValue data, int bitPos, bool value)
            => (ushort)unchecked(value ? (data.Value | (ushort)(0b1 << bitPos)) : (data.Value & (ushort) ~(0b1 << bitPos)));

        public DataValue(ushort value, byte size = 16)
            : this()
        {
            Value = value;
            _size = size;
        }

        public DataValue(byte upper, byte lower)
            : this()
        {
            LowerByte = lower;
            UpperByte = upper;
        }

        public bool Equals(DataValue other)
            => Value == other.Value;

        public override bool Equals(object obj)
            => obj is DataValue other && Equals(other);

        public override int GetHashCode()
            => Value.GetHashCode();

        public IEnumerator<(int pos, int pow, bool value)> GetEnumerator()
        {
            var t = this;
            return Enumerable.Range(0, 16).Select(i => (i, (int)Math.Pow(2, i), t.GetAt(i))).Reverse().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public static implicit operator DataValue(ushort value)
            => new DataValue(value);

        public static implicit operator DataValue(bool value)
            => value ? new DataValue(0xff, 0xff) : new DataValue(0);

        public static implicit operator bool(DataValue value)
            => value.Value != 0;

        public static implicit operator ushort(DataValue value)
            => value.Value;

        public static bool operator ==(DataValue value1, DataValue value2)
            => value1.Equals(value2);

        public static DataValue operator |(DataValue value1, DataValue value2)
            => (ushort)(value1.Value | value2.Value);

        public static bool operator !=(DataValue value1, DataValue value2)
            => !value1.Equals(value2);
    }
}
