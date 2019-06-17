using System.Collections.Generic;

namespace HardwareSimulator.Core
{
    public abstract class BuiltInGate : Gate
    {
        protected BuiltInGate(string name, IEnumerable<string> inputs, IEnumerable<string> outputs)
            : base(name, inputs, outputs)
        { }
    }
}
