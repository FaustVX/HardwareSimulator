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

        protected override Dictionary<string, bool?> Execute(Dictionary<string, bool?> inputs)
            => new Dictionary<string, bool?>() { ["out"] = inputs["in"].HasValue ? Execute(inputs["in"].Value) : new bool?() };
    }
}
