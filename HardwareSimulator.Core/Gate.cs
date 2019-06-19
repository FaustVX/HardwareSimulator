using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HardwareSimulator.Core
{
    public abstract class Gate
    {
        private static Dictionary<string, Func<Gate>> Gates { get; } = new Dictionary<string, Func<Gate>>();
        private static Dictionary<string, Gate> NoStatedGates { get; } = new Dictionary<string, Gate>();

        public IReadOnlyList<string> Inputs { get; }
        public IReadOnlyList<string> Outputs { get; }
        public string Name { get; }

        protected ISet<string> Connectors { get; }
        public bool IsStated { get; }

        protected Gate(string name, IEnumerable<string> inputs, IEnumerable<string> outputs, bool stated = false)
        {
            var connectors = inputs.Concat(outputs);
            Connectors = new HashSet<string>();
            foreach (var connector in connectors)
                if (!Connectors.Add(connector))
                    throw new System.Exception($"Connector '{connector}' is already a connector");

            Inputs = new List<string>(inputs).AsReadOnly();
            Outputs = new List<string>(outputs).AsReadOnly();
            Name = name;
            IsStated = stated;
        }

        public static void RegisterGate(string name, Func<Gate> gate)
            => Gates[name.ToLower()] = gate;

        public static void RegisterGate(string name, Gate gate)
            => NoStatedGates[name.ToLower()] = gate;

        public static void RegisterGate<TGate>()
            where TGate : Gate, new()
        {
            var gate = new TGate();
            if (gate.IsStated)
                RegisterGate(gate.Name, () => new TGate());
            else
                RegisterGate(gate.Name, gate);
        }

        public static bool TryGetGate(string name, out Gate gate)
        {
            if (NoStatedGates.TryGetValue(name, out gate))
                return true;

            var ok = Gates.TryGetValue(name, out var g);
            if(ok)
                gate = g();
            return ok;
        }

        protected static Gate GetGate(string name)
        {
            name = name.ToLower();
            if (NoStatedGates.TryGetValue(name, out var gate))
                return gate;
            return Gates[name]();
        }

        public static void ClearGates()
        {
            Gates.Clear();
            NoStatedGates.Clear();
        }

        protected abstract Dictionary<string, DataValue?> Execute(Dictionary<string, DataValue?> inputs);

        public IReadOnlyDictionary<string, DataValue?> Execute(params (string name, DataValue? value)[] inputs)
        {
            var dict = new Dictionary<string, DataValue?>();
            foreach (var (name, value) in inputs)
                if (!Inputs.Contains(name))
                    throw new System.Exception($"'{name}' is not an input Connector");
                else if (dict.ContainsKey(name))
                    throw new System.Exception($"'{name}' is already defined");
                else
                    dict.Add(name, value);

            foreach (var input in Inputs.Where(i => !dict.ContainsKey(i)))
                dict.Add(input, false);

            dict = Execute(dict);
            foreach (var o in Outputs.Where(o => !dict.ContainsKey(o)))
                dict.Add(o, null);
            return new ReadOnlyDictionary<string, DataValue?>(dict);
        }
    }
}
