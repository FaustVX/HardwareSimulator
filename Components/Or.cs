namespace HardwareSimulator
{
    public sealed class Or : ABOutGate
    {
        public Or()
            : base(nameof(Or))
        { }

        public override bool Execute(bool a, bool b)
            => a || b;
    }
}
