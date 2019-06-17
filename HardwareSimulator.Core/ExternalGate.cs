using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;

namespace HardwareSimulator.Core
{
    public sealed class ExternalGate : Gate
    {
        private delegate Dictionary<string, bool?> _Execute(ExternalGate @this, Dictionary<string, bool?> inputs);
        private static readonly Regex _regexFile, _regexContent, _regexConnectors;

        private readonly _Execute _execute;
        private List<(Gate gate, IGrouping<string, string>[] inputs, IGrouping<string, string>[] outputs)> Parts;

        static ExternalGate()
        {
            var name = "[a-zA-Z_][a-zA-Z0-9_]*";
            _regexFile = new Regex(@"^CHIP\s+(" + name + @")\s*\n*{\r*\n*((?:.*\n)*?)\r*\n*}$", RegexOptions.Multiline);
            _regexContent = new Regex(@"IN\s+(.*?;).*?OUT\s+(.*?;).*?PARTS:\r?\n?(.*)", RegexOptions.Singleline);
            _regexConnectors = new Regex("(" + name + @")(?:\[(\d+)\])?[,;]");
        }

        private ExternalGate(string name, IEnumerable<string> inputs, IEnumerable<string> outputs, DirectoryInfo directory, string[] partsString)
            : base(name, inputs, outputs)
        {
            _execute = Parse(directory, partsString);
            RegisterGate(this);
        }

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

            if (Gates.TryGetValue(name, out var g) && g is ExternalGate gate)
                return gate;

            var content = _regexContent.Match(regex.Groups[2].Value);
            var ins = _regexConnectors.Matches(content.Groups[1].Value).Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            var outs = _regexConnectors.Matches(content.Groups[2].Value).Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            var parts = content.Groups[3].Value;

            return new ExternalGate(name, ins, outs, new FileInfo(file).Directory, parts.Split("\r\n".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray());
        }

        private _Execute Parse(DirectoryInfo directory, string[] partsString)
        {
            var parts = new List<(Gate gate, IGrouping<string, string>[] inputs, IGrouping<string, string>[] outputs)>(partsString.Length);

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

                var gate = Gates.TryGetValue(name.ToLower(), out var g) ? g : Parse(Path.Combine(directory.FullName, name + ".hdl"));

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
                    if (Inputs.Contains(input.Key))
                        return -1;

                foreach (var input in t2.inputs)
                    if (Inputs.Contains(input.Key))
                        return 1;

                foreach (var output in t1.outputs)
                    if (Outputs.Contains(output.Key))
                        return -1;

                foreach (var output in t2.outputs)
                    if (Outputs.Contains(output.Key))
                        return 1;

                return 0;
            });

            Parts = parts;

            return (t, dic) =>
            {
                foreach (var (gate, inputs, outputs) in t.Parts)
                    foreach (var result in gate.Execute((inputs).Where(input => dic.ContainsKey(input.Key)).SelectMany(input => input.Select(i => (i, dic[input.Key]))).ToArray()))
                        foreach (var output in outputs.Where(output => output.Key == result.Key).SelectMany(output => output))
                            dic.Add(output, result.Value);
                return dic.Where(kvp => t.Outputs.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            };
        }
    }
}
