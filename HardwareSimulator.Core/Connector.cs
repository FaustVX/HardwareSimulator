namespace HardwareSimulator.Core
{
    public abstract class Connector
    {
        protected Connector(string name)
        {
            Name = name;
            Value = false;
        }

        public string Name { get; }
        public bool? Value { get; set; }

        public override bool Equals(object obj)
            => obj is Connector c && c.Name == Name;

        public override int GetHashCode()
            => (Name, Value).GetHashCode();
    }
}
