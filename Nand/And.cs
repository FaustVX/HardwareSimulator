namespace HardwareSimulator
{
    public sealed class And : ABOutGate
    {
        public And()
            : base(nameof(And))
        { }

        public override bool Execute(bool a, bool b)
            => a && b;
    }
}
