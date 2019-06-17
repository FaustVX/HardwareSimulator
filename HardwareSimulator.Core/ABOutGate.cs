using HardwareSimulator.Core;
using System.Collections.Generic;

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
