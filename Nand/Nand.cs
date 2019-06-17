using HardwareSimulator.Core;
using System.Collections.Generic;

namespace HardwareSimulator
{
    public sealed class Nand : BuiltInGate
    {
        public Nand()
            : base("nand", new[] { "a", "b" }, new[] { "out" })
        { }

        public bool Execute(bool a, bool b)
            => !(a && b);

        protected override IReadOnlyDictionary<string, bool?> Execute(Dictionary<string, bool?> inputs)
            => new Dictionary<string, bool?>() { ["out"] = (inputs["a"].HasValue && inputs["b"].HasValue) ? Execute(inputs["a"].Value, inputs["b"].Value) : new bool?() };
    }
}
