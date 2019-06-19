using HardwareSimulator.Core;
using System.Collections.Generic;

namespace HardwareSimulator
{
    public sealed class Not : BuiltInGate
    {
        public Not()
            : base(nameof(Not), new[] { "in" }, new[] { "out" })
        { }

        public bool Execute(bool a)
            => !a;

        protected override Dictionary<string, DataValue?> Execute(Dictionary<string, DataValue?> inputs)
            => new Dictionary<string, DataValue?>() { ["out"] = inputs["in"].HasValue ? Execute(inputs["in"].Value) : new DataValue?() };
    }
}
