namespace HardwareSimulator
{
    public sealed class Clock : InOutGate
    {
        public Clock()
            : base(nameof(Clock))
        { }

        private bool last = false;

        public override bool Execute(bool @in)
            => !last & (last = @in);
    }
}
