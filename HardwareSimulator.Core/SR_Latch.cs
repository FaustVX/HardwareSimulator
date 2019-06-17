using HardwareSimulator.Core;
using System.Collections.Generic;

namespace HardwareSimulator
{
    public abstract class SR_Latche : BuiltInGate
    {
        protected SR_Latche()
            : base(nameof(SR_Latche), new[] { "s", "r" }, new[] { "out", "inv" }, stated: true)
        { }

        public abstract bool Execute(bool a, bool b);
        private bool? _out;

        protected override Dictionary<string, bool?> Execute(Dictionary<string, bool?> inputs)
        {
            var s = inputs["s"];
            var r = inputs["r"];
            if (s.HasValue && r.HasValue || s.Value && r.Value)
            {
                if (s.Value)
                    _out = true;
                else if (r.Value)
                    _out = false;
            }
            else
                _out = null;

            return new Dictionary<string, bool?>(2)
            {
                ["out"] = _out,
                ["inv"] = _out.HasValue ? !_out.Value : _out
            };
        }
    }
}
