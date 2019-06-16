using System;
using System.Collections.Generic;
using System.Text;

namespace HardwareSimulator.Core
{
    public sealed class Link
    {
        public Link(InputConnector input, IEnumerable<OutputConnector> outputs)
        {
            Input = input;
            Outputs = outputs;
        }

        public InputConnector Input { get; }
        public IEnumerable<OutputConnector> Outputs { get; }

        public void Update()
        {
            foreach (var output in Outputs)
                output.Value = Input.Value;
        }
    }
}
