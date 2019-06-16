using System.Collections.Generic;

namespace HardwareSimulator.Core
{
    public abstract class BuiltInGate : Gate
    {
        protected BuiltInGate(IEnumerable<InputConnector> inputs, IEnumerable<OutputConnector> outputs)
            : base(inputs, outputs)
        {
        }
    }
}
