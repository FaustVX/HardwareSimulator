﻿using System;
using System.Runtime.InteropServices;

namespace HardwareSimulator.Core
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct DataValue : IEquatable<DataValue>
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

        public static bool GetAt(in DataValue data, int bitPos)
            => ((data.Value >> bitPos) & 0b1) == 0b1;

        public static DataValue SetAt(in DataValue data, int bitPos, bool value)
            => (ushort)unchecked(value ? (data.Value | (ushort)(0b1 << bitPos)) : (data.Value & (ushort) ~(0b1 << bitPos)));

        public DataValue(ushort value)
            : this()
        {
            Value = value;
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

        public static implicit operator DataValue(ushort value)
            => new DataValue(value);

        public static implicit operator DataValue(bool value)
            => value ? new DataValue(0xff, 0xff) : new DataValue(0);

        public static implicit operator bool(DataValue value)
            => value.Value != 0;

        public static implicit operator ushort(DataValue value)
            => value.Value;

        public static implicit operator byte(DataValue value)
            => value.LowerByte;
            //=> value.UpperByte == 0 ? value.LowerByte : throw new System.Exception($"{nameof(UpperByte)} must be '0'");

        public static bool operator ==(DataValue value1, DataValue value2)
            => value1.Equals(value2);

        public static bool operator !=(DataValue value1, DataValue value2)
            => !value1.Equals(value2);
    }
}
