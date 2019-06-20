using HardwareSimulator.Core;
using System.Collections.Generic;
#if Computer8Bits
using DataValue = HardwareSimulator.Core.DataValue8Bits;
using InnerType = System.Byte;
#elif Computer16Bits
using DataValue = HardwareSimulator.Core.DataValue16Bits;
using InnerType = System.UInt16;
#endif

namespace HardwareSimulator
{
    public abstract class InOutGate : BuiltInGate
    {
        protected InOutGate(string name, bool stated = false)
            : base(name, new[] { "in" }, new[] { "out" }, stated)
        { }

        public abstract bool Execute(bool @in);

        public DataValue? Execute(DataValue? @in)
            => @in.HasValue ? Execute(@in.Value) : new DataValue?();

        protected override Dictionary<string, DataValue?> Execute(Dictionary<string, DataValue?> inputs)
            => new Dictionary<string, DataValue?>() { ["out"] = Execute(inputs["in"]) };
    }
}
