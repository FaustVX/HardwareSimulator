namespace HardwareSimulator
{
    public sealed class Xor : ABOutGate
    {
        public Xor()
            : base(nameof(Xor))
        { }

        public override bool Execute(bool a, bool b)
            => a ^ b;
    }
}
