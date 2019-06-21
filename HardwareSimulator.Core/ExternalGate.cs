using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using Connector = System.Linq.IGrouping<string, string>;
#if Computer8Bits
using DataValue = HardwareSimulator.Core.DataValue8Bits;
using InnerType = System.Byte;
#elif Computer16Bits
using DataValue = HardwareSimulator.Core.DataValue16Bits;
using InnerType = System.UInt16;
#endif

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
            var span = @"(?:\[(0*[0-9]|0*1[0-6]?)(?:\.\.(0*[0-9]|0*1[0-6]?))?\])";
            _regexFile = new Regex(@"^CHIP\s+(" + name + @")\s*\n*{\r*\n*((?:.*\n)*?)\r*\n*}$", RegexOptions.Multiline);
            _regexContent = new Regex(@"(STATED;.*?)?IN\s+(.*?;).*?OUT\s+(.*?;).*?(?:CLOCKED\s+(.*?;).*?)?(?:BUS\s+(.*?;).*?)?PARTS:\r?\n?(.*)", RegexOptions.Singleline);
            _regexConnectors = new Regex("(" + name + ")" + array + "?[,;]");
            //_regexConnectors = new Regex("(" + name + ")[,;]");
            _regexPart = new Regex(name + @"\s*\(\s*(?:" + name + span + @"?\s*=\s*" + name + span + @"?\s*,?\s*)+\)\s*;");
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
                ProcessResults(gate.Execute(GetInputs(ins).GroupBy(t => t.name, t => t.value).Select(n => (n.Key, n.Aggregate((prev, next) => prev | next))).ToArray()), outs);

            inputs.Remove("true");
            inputs.Remove("false");
            return inputs;//.Where(kvp => t.Outputs.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            IEnumerable<(string name, DataValue? value)> GetInputs(Connector[] ins)
            {
                foreach (var input in ins)
                {
                    var split1 = input.Key.Split("[.]".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                    var name1 = split1[0];

                    if (inputs.ContainsKey(name1))
                    {
                        foreach (var i in input)
                        {
                            var split2 = i.Split("[.]".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                            var name2 = split2[0];


                            if (split2.Length == 2 && byte.TryParse(split2[1], out var pos2) && pos2 < DataValue.MaxBits)
                            {
                                var result = new DataValue?();

                                if (inputs[name1] is null)
                                    yield return (name2, null);
                                else
                                {
                                    if (split1.Length == 2 && byte.TryParse(split1[1], out var pos) && pos < DataValue.MaxBits)
                                        result = (DataValue)((inputs[name1].Value.GetAt(pos) ? 0b1ul : 0b0ul) << pos2);
                                    else if (split1.Length == 3 && byte.TryParse(split1[1], out var start) && start < DataValue.MaxBits && byte.TryParse(split1[2], out var end) && end < DataValue.MaxBits && end > start)
                                        throw new System.Exception();
                                    else if (split1.Length == 1)
                                        result = (DataValue)((inputs[name1].Value.GetAt(0) ? 0b1ul : 0b0ul) << pos2);
                                    else
                                        throw new System.Exception();

                                    yield return (name2, result);
                                }
                            }
                            else if (split2.Length == 3 && byte.TryParse(split2[1], out var start2) && start2 < DataValue.MaxBits && byte.TryParse(split2[2], out var end2) && end2 < DataValue.MaxBits && end2 > start2)
                            {
                                var result = new DataValue?();

                                if (inputs[name1] is null)
                                    yield return (name2, null);
                                else
                                {
                                    if (split1.Length == 2 && byte.TryParse(split1[1], out var pos) && pos < DataValue.MaxBits)
                                        throw new System.Exception();
                                    else if (split1.Length == 3 && byte.TryParse(split1[1], out var start) && start < DataValue.MaxBits && byte.TryParse(split1[2], out var end) && end < DataValue.MaxBits && end > start && end - start == end2 - start2)
                                        result = (DataValue)(inputs[name1].Value.Splice(start, end) << start2);
                                    else if (split1.Length == 1)
                                        result = inputs[name1];
                                    else
                                        throw new System.Exception();

                                    yield return (name2, result);
                                }
                            }
                            else if (split2.Length == 1)
                            {
                                var result = new DataValue?();

                                if (inputs[name1] is null)
                                    yield return (name2, null);
                                else
                                {
                                    if (split1.Length == 2 && byte.TryParse(split1[1], out var pos) && pos < DataValue.MaxBits)
                                        result = inputs[name1].Value.GetAt(pos);
                                    else if (split1.Length == 3 && byte.TryParse(split1[1], out var start) && start < DataValue.MaxBits && byte.TryParse(split1[2], out var end) && end < DataValue.MaxBits && end > start)
                                        result = inputs[name1].Value.Splice(start, end);
                                    else if (split1.Length == 1)
                                        result = inputs[name1];
                                    else
                                        throw new System.Exception();

                                    yield return (name2, result);
                                }
                            }
                            else
                                throw new System.Exception();
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
                        var split = o.Split("[.]".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                        var name = split[0];
                        if (!Buses.Contains(name))
                        {
                            if (split.Length == 1)
                                inputs[o] = result.Value;
                            else if (split.Length == 2 && byte.TryParse(split[1], out var i) && i < DataValue.MaxBits)
                                inputs[name] = DataValue.SetAt(inputs.TryGetValue(name, out var value) ? (value?.Value ?? 0) : InnerType.MinValue, i, result.Value ?? false);
                            else if (split.Length == 3 && byte.TryParse(split[1], out var start) && start < DataValue.MaxBits && byte.TryParse(split[2], out var end) && end < DataValue.MaxBits && end > start)
                                inputs[name] = (inputs.TryGetValue(name, out var value) ? value : 0) | (InnerType)(result.Value.Value.Splice(end - start) << start);
                            else
                                throw new System.Exception();
                        }
                        else if (inputs.TryGetValue(name, out var input))
                        {
                            if (split.Length == 1)
                                inputs[o] = input | result.Value;
                            else if (split.Length == 2 && byte.TryParse(split[1], out var i) && i < DataValue.MaxBits)
                                inputs[name] = input | DataValue.SetAt(inputs.TryGetValue(name, out var value) ? (value?.Value ?? 0) : InnerType.MinValue, i, result.Value.Value);
                            else if (split.Length == 3 && byte.TryParse(split[1], out var start) && start < DataValue.MaxBits && byte.TryParse(split[2], out var end) && end < DataValue.MaxBits && end > start)
                                inputs[name] = input | (inputs.TryGetValue(name, out var value) ? value : 0) | (InnerType)(result.Value.Value.Splice(end - start) << start);
                            else
                                throw new System.Exception();
                        }
                        else
                        {
                            if (split.Length == 1)
                                inputs[o] = result.Value;
                            else if (split.Length == 2 && byte.TryParse(split[1], out var i) && i < DataValue.MaxBits)
                                inputs[name] = DataValue.SetAt(inputs.TryGetValue(name, out var value) ? (value?.Value ?? 0) : InnerType.MinValue, i, result.Value.Value);
                            else if (split.Length == 3 && byte.TryParse(split[1], out var start) && start < DataValue.MaxBits && byte.TryParse(split[2], out var end) && end < DataValue.MaxBits && end > start)
                                inputs[name] = (inputs.TryGetValue(name, out var value) ? value : 0) | (InnerType)(result.Value.Value.Splice(end - start) << start);
                            else
                                throw new System.Exception();
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
            var stated = content.Groups[1].Success;
            var ins = _regexConnectors.Matches(content.Groups[2].Value).Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            var outs = _regexConnectors.Matches(content.Groups[3].Value).Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            var parts = content.Groups[content.Groups.Count - 1].Value;
            var gates = Parse(ins, outs, new FileInfo(file).Directory, parts.Split("\r\n".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()));

            return stated || gates.Any(g => g.gate.IsStated) ? new StatedGate(name, ins, outs, gates) : new ExternalGate(name, ins, outs, gates);
        }

        private static List<(Gate gate, Connector[] inputs, Connector[] outputs)> Parse(string[] ins, string[] outs, DirectoryInfo directory, IEnumerable<string> partsString)
        {
            var parts = new List<(Gate gate, Connector[] inputs, Connector[] outputs)>();

            foreach (var part in partsString.Where(p => !string.IsNullOrWhiteSpace(p)))
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
                    var conn = connectors.First(c => !gate.Inputs.Contains(c.Item1.Split('[')[0]) && !gate.Outputs.Contains(c.Item1.Split('[')[0]));
                    throw new System.Exception($"'{conn.Item1}' isn't a connector in '{name}' gate");
                }
                catch (System.InvalidOperationException)
                {
                    ;
                }

                parts.Add((gate,
                    inputs:  connectors.Where(c => gate.Inputs.Contains(c.Item1.Split('[')[0])).GroupBy(c => c.Item2, c => c.Item1).ToArray(),
                    outputs: connectors.Where(c => gate.Outputs.Contains(c.Item1.Split('[')[0])).GroupBy(c => c.Item1, c => c.Item2).ToArray()));
            }

            return parts;

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
