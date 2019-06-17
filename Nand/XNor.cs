namespace HardwareSimulator
{
    public sealed class XNor : ABOutGate
    {
        public XNor()
            : base(nameof(XNor))
        { }

        public override bool Execute(bool a, bool b)
            => !(a ^ b);
    }
}
