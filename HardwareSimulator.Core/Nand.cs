namespace HardwareSimulator
{
    public sealed class Nand : ABOutGate
    {
        public Nand()
            : base("nand")
        { }

        public override bool Execute(bool a, bool b)
            => !(a && b);
    }
}
