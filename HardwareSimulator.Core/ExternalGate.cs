using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using Connector = System.Linq.IGrouping<string, string>;

namespace HardwareSimulator.Core
{
    public sealed class ExternalGate : Gate
    {
        private delegate Dictionary<string, bool?> _Execute(ExternalGate @this, Dictionary<string, bool?> inputs);
        private static readonly Regex _regexFile, _regexContent, _regexConnectors;

        private readonly _Execute _execute;
        private readonly List<(Gate gate, Connector[] inputs, Connector[] outputs)> Parts;

        static ExternalGate()
        {
            var name = "[a-zA-Z_][a-zA-Z0-9_]*";
            _regexFile = new Regex(@"^CHIP\s+(" + name + @")\s*\n*{\r*\n*((?:.*\n)*?)\r*\n*}$", RegexOptions.Multiline);
            _regexContent = new Regex(@"IN\s+(.*?;).*?OUT\s+(.*?;).*?PARTS:\r?\n?(.*)", RegexOptions.Singleline);
            _regexConnectors = new Regex("(" + name + @")(?:\[(\d+)\])?[,;]");
        }

        private ExternalGate(string name, IEnumerable<string> inputs, IEnumerable<string> outputs, bool stated, List<(Gate gate, Connector[] inputs, Connector[] outputs)> parts)
            : base(name, inputs, outputs, stated)
        {
            _execute = (t, dic) =>
            {
                foreach (var (gate, ins, outs) in t.Parts)
                    foreach (var result in gate.Execute((ins).Where(input => dic.ContainsKey(input.Key)).SelectMany(input => input.Select(i => (i, dic[input.Key]))).ToArray()))
                        foreach (var output in outs.Where(output => output.Key == result.Key).SelectMany(output => output))
                            dic.Add(output, result.Value);
                return dic.Where(kvp => t.Outputs.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            };
            RegisterGate(name, IsStated ? () => new ExternalGate(this, Parts.Select(p => p.gate.Name)) : new System.Func<ExternalGate>(()=> this));
            Parts = parts;
        }

        private ExternalGate(ExternalGate gate, IEnumerable<string> gates)
            : this(gate.Name, gate.Inputs, gate.Outputs, gate.IsStated, gates.Select(g => (GetGate(g), gate.Parts.First(p => p.gate.Name == g).inputs, gate.Parts.First(p => p.gate.Name == g).outputs)).ToList())
        { }

        protected override Dictionary<string, bool?> Execute(Dictionary<string, bool?> inputs)
            => _execute(this, inputs);

        public static ExternalGate Parse(string file)
        {
            var text = File.ReadAllText(file);

            text = Regex.Replace(text, @"\/\*.*?\*\/", "", RegexOptions.Singleline);
            text = Regex.Replace(text, @"\/\/.*?\n", "");
            text = text.Replace("\r\n", "\n");

            var regex = _regexFile.Match(text);
            var name = regex.Groups[1].Value;

            {
                if (TryGetGate(name, out var g) && g is ExternalGate gate)
                    return gate;
            }

            var content = _regexContent.Match(regex.Groups[2].Value);
            var ins = _regexConnectors.Matches(content.Groups[1].Value).Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            var outs = _regexConnectors.Matches(content.Groups[2].Value).Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            var parts = content.Groups[3].Value;
            var gates = Parse(ins, outs, new FileInfo(file).Directory, parts.Split("\r\n".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()));
            return new ExternalGate(name, ins, outs, gates.Any(g => g.gate.IsStated), gates);
        }

        private static List<(Gate gate, Connector[] inputs, Connector[] outputs)> Parse(string[] ins, string[] outs, DirectoryInfo directory, IEnumerable<string> partsString)
        {
            var parts = new List<(Gate gate, Connector[] inputs, Connector[] outputs)>();

            foreach (var part in partsString)
            {
                var name = part.Substring(0, part.IndexOf('('));
                var connectorsString = part.Substring(part.IndexOf('(')+1);
                connectorsString = connectorsString.Substring(0, connectorsString.IndexOf(");"));
                var connectors = connectorsString.Split(',')
                    .Select(c => c.Split('=')
                        .Select(s => s.Trim()))
                    .Select(c => (c.ElementAt(0), c.ElementAt(1)))
                    .ToArray();

                var gate = TryGetGate(name.ToLower(), out var g) ? g : Parse(Path.Combine(directory.FullName, name + ".hdl"));

                if (gate is null)
                    throw new System.Exception($"Gate {name} must exist");

                parts.Add((gate,
                    inputs:  connectors.Where(c => gate.Inputs.Contains(c.Item1)).GroupBy(c => c.Item2, c => c.Item1).ToArray(),
                    outputs: connectors.Where(c => gate.Outputs.Contains(c.Item1)).GroupBy(c => c.Item1, c => c.Item2).ToArray()));
            }

            parts.Sort((t1, t2) =>
            {
                foreach (var output in t1.outputs)
                    foreach (var input in t2.inputs)
                        if (output.Contains(input.Key))
                            return -1;

                foreach (var output in t2.outputs)
                    foreach (var input in t1.inputs)
                        if (output.Contains(input.Key))
                            return 1;

                foreach (var input in t1.inputs)
                    if (ins.Contains(input.Key))
                        return -1;

                foreach (var input in t2.inputs)
                    if (ins.Contains(input.Key))
                        return 1;

                foreach (var output in t1.outputs)
                    if (outs.Contains(output.Key))
                        return -1;

                foreach (var output in t2.outputs)
                    if (outs.Contains(output.Key))
                        return 1;

                return 0;
            });

            return parts;
        }
    }
}
