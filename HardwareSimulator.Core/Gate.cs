using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HardwareSimulator.Core
{
    public abstract class Gate
    {
        public IReadOnlyList<InputConnector> Inputs { get; }
        public IReadOnlyList<OutputConnector> Outputs { get; }

        protected IReadOnlyDictionary<string, Connector> Connectors { get; }

        protected Gate(IEnumerable<InputConnector> inputs, IEnumerable<OutputConnector> outputs)
        {
            var connectors = inputs.Cast<Connector>().Concat(outputs);
            Connectors = new ReadOnlyDictionary<string, Connector>(connectors.ToDictionary(c => c.Name));
            Inputs = new List<InputConnector>(inputs).AsReadOnly();
            Outputs = new List<OutputConnector>(outputs).AsReadOnly();
        }

        public abstract void Update();
    }
}
