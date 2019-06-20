using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using Connector = System.Linq.IGrouping<string, string>;

namespace HardwareSimulator.Core
{
    public class ExternalGate : Gate
    {
        private sealed class StatedGate : ExternalGate
        {
            public StatedGate(string name, IEnumerable<string> inputs, IEnumerable<string> outputs, List<(Gate gate, Connector[] inputs, Connector[] outputs)> parts)
                : base(name, inputs, outputs, true, parts)
            {
                InternalConnectors = new Dictionary<string, DataValue?>();
            }

            public StatedGate(ExternalGate gate)
                : this(gate.Name, gate.Inputs, gate.Outputs, gate.Parts.Select(p => (GetGate(p.gate.Name), p.inputs, p.outputs)).ToList())
            { }

            public Dictionary<string, DataValue?> InternalConnectors { get; }

            protected override Dictionary<string, DataValue?> Execute(Dictionary<string, DataValue?> inputs)
            {
                Combine(inputs, InternalConnectors, false);
                var executed = base.Execute(inputs);
                Combine(InternalConnectors, executed, true);
                return executed;
            }

            public static void Combine<Tkey, TValue>(Dictionary<Tkey, TValue> source, IReadOnlyDictionary<Tkey, TValue> others, bool replace)
            {
                foreach (var item in others)
                    if (replace || !source.ContainsKey(item.Key))
                        source[item.Key] = item.Value;
            }
        }
        
        private static readonly Regex _regexFile, _regexContent, _regexConnectors, _regexPart;
        
        private readonly List<(Gate gate, Connector[] inputs, Connector[] outputs)> Parts;

        static ExternalGate()
        {
            var name = "[a-zA-Z_][a-zA-Z0-9_]*";
            var array = @"(?:\[([1-9]|1[0-6]?)\])";
            var array0 = @"(?:\[(0*[0-9]|0*1[0-6]?)\])";
            _regexFile = new Regex(@"^CHIP\s+(" + name + @")\s*\n*{\r*\n*((?:.*\n)*?)\r*\n*}$", RegexOptions.Multiline);
            _regexContent = new Regex(@"IN\s+(.*?;).*?OUT\s+(.*?;).*?PARTS:\r?\n?(.*)", RegexOptions.Singleline);
            _regexConnectors = new Regex("(" + name + ")" + array + "?[,;]");
            //_regexConnectors = new Regex("(" + name + ")[,;]");
            _regexPart = new Regex(name + @"\s*\(\s*(?:" + name + array0 + @"?\s*=\s*" + name + array0 + @"?\s*,?\s*)+\)\s*;");
        }

        private ExternalGate(string name, IEnumerable<string> inputs, IEnumerable<string> outputs, List<(Gate gate, Connector[] inputs, Connector[] outputs)> parts)
            : this(name, inputs, outputs, false, parts)
        { }

        private ExternalGate(string name, IEnumerable<string> inputs, IEnumerable<string> outputs, bool stated, List<(Gate gate, Connector[] inputs, Connector[] outputs)> parts)
            : base(name, inputs, outputs, stated)
        {
            Parts = parts;

            if (IsStated)
                RegisterGate(name, () => new StatedGate(this));
            else
                RegisterGate(name, this);
        }

        protected override Dictionary<string, DataValue?> Execute(Dictionary<string, DataValue?> inputs)
        {
            inputs["true"] = true;
            inputs["false"] = false;

            foreach (var (gate, ins, outs) in Parts)
                ProcessResults(gate.Execute(GetInputs(ins).ToArray()), outs);

            inputs.Remove("true");
            inputs.Remove("false");
            return inputs;//.Where(kvp => t.Outputs.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            IEnumerable<(string name, DataValue? value)> GetInputs(Connector[] ins)
            {
                foreach (var input in ins)
                {
                    var split = input.Key.Split("[]".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                    var name = split[0];

                    if (inputs.ContainsKey(name))
                    {
                        foreach (var i in input)
                        {
                            if (split.Length == 2 && int.TryParse(split[1], out var pos))
                                if (inputs[name].HasValue)
                                    yield return (i, inputs[name].Value.GetAt(pos));
                                else
                                    yield return (i, null);
                            else
                                yield return (i, inputs[name]);
                        }
                    }
                }
            }

            void ProcessResults(IReadOnlyDictionary<string, DataValue?> results, Connector[] outs)
            {
                foreach (var result in results)
                {
                    foreach (var output in outs)
                    {
                        if(output.Key == result.Key)
                        {
                            foreach (var o in output)
                            {
                                var split = o.Split("[]".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                                var name = split[0];
                                if (split.Length == 1)
                                    inputs[o] = result.Value;
                                else
                                    inputs[name] = DataValue.SetAt(inputs.TryGetValue(name, out var value) ? (value?.Value ?? 0) : ushort.MinValue, int.Parse(split[1]), result.Value.Value);
                            }
                        }
                    }
                }
            }
        }

        public static ExternalGate Parse(string file)
        {
            var text = File.ReadAllText(file);

            text = Regex.Replace(text, @"\/\*.*?\*\/", "", RegexOptions.Singleline);
            text = Regex.Replace(text, @"\/\/.*?\n", "");
            text = text.Replace("\r\n", "\n");

            var regex = _regexFile.Match(text);

            if (!regex.Success)
                throw new System.Exception($"{Path.GetFileName(file)} can't be parsed");

            var name = regex.Groups[1].Value;

            if (name != Path.GetFileNameWithoutExtension(file))
                throw new System.Exception($"Chip name not equals the file name. Actual:'{name}', expected:'{Path.GetFileNameWithoutExtension(file)}'");
            
            var content = _regexContent.Match(regex.Groups[2].Value);
            var ins = _regexConnectors.Matches(content.Groups[1].Value).Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            var outs = _regexConnectors.Matches(content.Groups[2].Value).Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            var parts = content.Groups[3].Value;
            var gates = Parse(ins, outs, new FileInfo(file).Directory, parts.Split("\r\n".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()));

            return gates.Any(g => g.gate.IsStated) ? new StatedGate(name, ins, outs, gates) : new ExternalGate(name, ins, outs, gates);
        }

        private static List<(Gate gate, Connector[] inputs, Connector[] outputs)> Parse(string[] ins, string[] outs, DirectoryInfo directory, IEnumerable<string> partsString)
        {
            var parts = new List<(Gate gate, Connector[] inputs, Connector[] outputs)>();

            foreach (var part in partsString)
            {
                if (!_regexPart.IsMatch(part))
                    throw new System.Exception($"'{part}' is an invalid part call");

                var name = part.Substring(0, part.IndexOf('('));

                var gate = TryGetGate(name.ToLower(), out var g) ? g : Parse(Path.Combine(directory.FullName, name + ".hdl"));

                if (gate is null)
                    throw new System.Exception($"Gate {name} must exist");

                var connectorsString = part.Substring(part.IndexOf('(')+1);
                connectorsString = connectorsString.Substring(0, connectorsString.IndexOf(");"));
                var connectors = connectorsString.Split(',')
                    .Select(c => c.Split('=')
                        .Select(s => s.Trim()))
                    .Select(c => (c.ElementAt(0), c.ElementAt(1)))
                    .ToArray();

                try
                {
                    var conn = connectors.First(c => !gate.Inputs.Contains(c.Item1) && !gate.Outputs.Contains(c.Item1));
                    throw new System.Exception($"'{conn.Item1}' isn't a connector in '{name}' gate");
                }
                catch (System.InvalidOperationException)
                {
                    ;
                }

                parts.Add((gate,
                    inputs:  connectors.Where(c => gate.Inputs.Contains(c.Item1)).GroupBy(c => c.Item2, c => c.Item1).ToArray(),
                    outputs: connectors.Where(c => gate.Outputs.Contains(c.Item1)).GroupBy(c => c.Item1, c => c.Item2).ToArray()));
            }

            parts.Sort((t1, t2) =>
            {
                var t1Int2 = AInB(t1, t2);
                var t2Int1 = AInB(t2, t1);

                if (t1Int2 && !t2Int1)
                    return -1;

                if (t2Int1 && !t1Int2)
                    return 1;

                var inputsInT1 = InputsInA(t1);
                var inputsInT2 = InputsInA(t2);

                if (inputsInT1 && !inputsInT2)
                    return -1;

                if (inputsInT2 && !inputsInT1)
                    return 1;

                var t1InOutputs = AInOutputs(t1);
                var t2InOutputs = AInOutputs(t2);

                if (t1InOutputs && !t2InOutputs)
                    return 1;

                if (t2InOutputs && !t1InOutputs)
                    return -1;

                return 0;

                bool AInB((Gate, Connector[] inputs, Connector[] outputs) a, (Gate, Connector[] inputs, Connector[] outputs) b)
                {
                    foreach (var output in a.outputs)
                        foreach (var input in b.inputs)
                            if (output.Contains(input.Key))
                                return true;
                    return false;
                }

                bool InputsInA((Gate, Connector[] inputs, Connector[] outputs) a)
                {
                    foreach (var input in a.inputs)
                        if (ins.Contains(input.Key))
                            return true;
                    return false;
                }

                bool AInOutputs((Gate, Connector[] inputs, Connector[] outputs) a)
                {
                    foreach (var output in a.outputs)
                        if (outs.Contains(output.Key))
                            return true;
                    return false;
                }
            });

            return parts;
        }
    }
}
