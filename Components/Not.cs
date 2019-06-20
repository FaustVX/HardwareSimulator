namespace HardwareSimulator
{
    public sealed class Not : InOutGate
    {
        public Not()
            : base(nameof(Not))
        { }

        public override bool Execute(bool @in)
            => !@in;
    }
}
