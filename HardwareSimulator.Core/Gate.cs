using System.Collections.Generic;
using System.Linq;

namespace HardwareSimulator.Core
{
    public abstract class Gate
    {
        public static Dictionary<string, Gate> Gates { get; } = new Dictionary<string, Gate>();

        public IReadOnlyList<string> Inputs { get; }
        public IReadOnlyList<string> Outputs { get; }
        public string Name { get; }

        protected ISet<string> Connectors { get; }

        protected Gate(string name, IEnumerable<string> inputs, IEnumerable<string> outputs)
        {
            var connectors = inputs.Concat(outputs);
            Connectors = new HashSet<string>();
            foreach (var connector in connectors)
                if (!Connectors.Add(connector))
                    throw new System.Exception($"Connector '{connector}' is already a connector");

            Inputs = new List<string>(inputs).AsReadOnly();
            Outputs = new List<string>(outputs).AsReadOnly();
            Name = name;
        }

        public static void RegisterGate(Gate gate)
            => Gates[gate.Name] = gate;

        protected abstract IReadOnlyDictionary<string, bool?> Execute(Dictionary<string, bool?> inputs);

        public IReadOnlyDictionary<string, bool?> Execute(params (string name, bool? value)[] inputs)
        {
            var dict = new Dictionary<string, bool?>();
            foreach (var (name, value) in inputs)
                if (dict.ContainsKey(name))
                    throw new System.Exception();
                else
                    dict.Add(name, value);
            return Execute(dict);
        }
    }
}
