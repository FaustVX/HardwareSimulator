using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;

namespace HardwareSimulator.Core
{
    public sealed class ExternalGate : Gate
    {
        private delegate IReadOnlyDictionary<string, bool?> _Execute(Dictionary<string, bool?> inputs);
        private static readonly Regex _regexFile, _regexContent, _regexConnectors;

        private readonly _Execute _execute;

        static ExternalGate()
        {
            _regexFile = new Regex(@"^CHIP\s+([a-zA-Z0-9]*)\s*\n*{\r*\n*((?:.*\n)*?)\r*\n*}$", RegexOptions.Multiline);
            _regexContent = new Regex(@"IN\s+(.*?;).*?OUT\s+(.*?;).*?PARTS:\r?\n?(.*)", RegexOptions.Singleline);
            _regexConnectors = new Regex(@"([_a-zA-Z][_a-zA-Z0-9]*)(?:\[(\d+)\])?[,;]");
        }

        private ExternalGate(string name, IEnumerable<string> inputs, IEnumerable<string> outputs, _Execute execute)
            : base(name, inputs, outputs)
        {
            _execute = execute;
            RegisterGate(this);
        }

        protected override IReadOnlyDictionary<string, bool?> Execute(Dictionary<string, bool?> inputs)
            => _execute(inputs);

        public static ExternalGate Parse(string file)
        {
            var text = File.ReadAllText(file);

            text = Regex.Replace(text, @"\/\*.*?\*\/", "", RegexOptions.Singleline);
            text = Regex.Replace(text, @"\/\/.*?\n", "");
            text = text.Replace("\r\n", "\n");

            var regex = _regexFile.Match(text);
            var name = regex.Groups[1].Value;

            var content = _regexContent.Match(regex.Groups[2].Value);
            var ins = _regexConnectors.Matches(content.Groups[1].Value).Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            var outs = _regexConnectors.Matches(content.Groups[2].Value).Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            var parts = content.Groups[3].Value;

            return new ExternalGate(name, ins, outs, Parse(new FileInfo(file).Directory, parts.Split("\r\n".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray()));
        }

        private static _Execute Parse(DirectoryInfo directory, string[] partsString)
        {
            var parts = new Dictionary<int, (Gate gate, IGrouping<string, string>[] inputs, IGrouping<string, string>[] outputs)>(partsString.Length);

            foreach (var part in partsString)
            {
                var name = part.Substring(0, part.IndexOf('('));
                var connectorsString = part.Substring(part.IndexOf('(')+1);
                connectorsString = connectorsString.Substring(0, connectorsString.IndexOf(')'));
                var connectors = connectorsString.Split(',')
                    .Select(c => c.Split('=')
                        .Select(s => s.Trim()))
                    .Select(c => (c.ElementAt(0), c.ElementAt(1)))
                    .ToArray();

                var gate = Gates.TryGetValue(name.ToLower(), out var g) ? g : Parse(Path.Combine(directory.FullName, name + ".hdl"));

                if (gate is null)
                    throw new System.Exception($"Gate {name} must exist");

                parts.Add(parts.Count, (gate,
                    inputs:  connectors.Where(c => gate.Inputs.Contains(c.Item1)).GroupBy(c => c.Item2, c => c.Item1).ToArray(),
                    outputs: connectors.Where(c => gate.Outputs.Contains(c.Item1)).GroupBy(c => c.Item1, c => c.Item2).ToArray()));
            }

            return null;
        }
    }
}
