using HardwareSimulator.Core;
using System.Collections.Generic;

namespace HardwareSimulator
{
    public sealed class SR_Latch : BuiltInGate
    {
        public SR_Latch()
            : base(nameof(SR_Latch), new[] { "s", "r" }, new[] { "out", "inv" }, stated: true)
        { }

        private DataValue? _out = false;

        protected override Dictionary<string, DataValue?> Execute(Dictionary<string, DataValue?> inputs)
        {
            var s = inputs["s"];
            var r = inputs["r"];
            if (s.HasValue && r.HasValue && !(s.Value && r.Value))
            {
                if (s.Value)
                    _out = true;
                else if (r.Value)
                    _out = false;
            }
            else
                _out = null;

            return new Dictionary<string, DataValue?>(2)
            {
                ["out"] = _out,
                ["inv"] = _out.HasValue ? !_out.Value : _out
            };
        }
    }
}
