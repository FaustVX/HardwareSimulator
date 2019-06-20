using HardwareSimulator.Core;
using System.Collections.Generic;

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
