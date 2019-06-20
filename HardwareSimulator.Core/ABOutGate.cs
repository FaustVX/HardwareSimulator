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
    public abstract class ABOutGate : BuiltInGate
    {
        protected ABOutGate(string name, bool stated = false)
            : base(name, new[] { "a", "b" }, new[] { "out" }, stated)
        { }

        public abstract bool Execute(bool a, bool b);

        public DataValue? Execute(DataValue? a, DataValue? b)
            => (a.HasValue && b.HasValue) ? Execute(a.Value, b.Value) : new DataValue?();

        protected override Dictionary<string, DataValue?> Execute(Dictionary<string, DataValue?> inputs)
            => new Dictionary<string, DataValue?>() { ["out"] = Execute(inputs["a"], inputs["b"]) };
    }
}
