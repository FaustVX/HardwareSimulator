using HardwareSimulator.Core;

namespace HardwareSimulator
{
    public sealed class Nand : BuiltInGate
    {
        public Nand()
            : base(new[] { new InputConnector("a"), new InputConnector("b") }, new[] { new OutputConnector("out") })
        {
            A   = (InputConnector) Connectors["a"];
            B   = (InputConnector) Connectors["b"];
            Out = (OutputConnector)Connectors["out"];
        }

        public InputConnector A { get; }
        public InputConnector B { get; }
        public OutputConnector Out { get; }

        public override void Update()
            => Out.Value = (A.Value.HasValue && B.Value.HasValue) ? !(A.Value.Value && B.Value.Value) : new bool?();
    }
}
