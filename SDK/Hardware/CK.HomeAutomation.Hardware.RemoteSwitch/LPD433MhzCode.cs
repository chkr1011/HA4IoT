namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public class LPD433MhzCode
    {
        public LPD433MhzCode(ulong value, int length)
        {
            Value = value;
            Length = length;
        }

        public ulong Value { get; }

        public int Length { get; }
    }
}
