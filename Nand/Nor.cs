namespace HardwareSimulator
{
    public sealed class Nor : ABOutGate
    {
        public Nor()
            : base(nameof(Nor))
        { }

        public override bool Execute(bool a, bool b)
            => !(a || b);
    }
}
