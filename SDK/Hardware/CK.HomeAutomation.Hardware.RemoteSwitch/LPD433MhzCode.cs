namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public class LPD433MhzCode
    {
        public LPD433MhzCode(uint value, int length)
        {
            Value = value;
            Length = length;
        }

        public uint Value { get; }

        public int Length { get; }
    }
}
