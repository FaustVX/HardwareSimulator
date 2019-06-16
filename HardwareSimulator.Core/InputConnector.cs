using System.Collections.Generic;

namespace HardwareSimulator.Core
{
    public sealed class InputConnector : Connector
    {
        public InputConnector(string name)
            : base(name)
        { }

        public OutputConnector Output { get; private set; }

        public void ConnectTo(OutputConnector output)
            => Output = output;
    }
}
