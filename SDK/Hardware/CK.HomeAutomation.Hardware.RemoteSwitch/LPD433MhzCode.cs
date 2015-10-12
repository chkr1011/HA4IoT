namespace CK.HomeAutomation.Hardware.RemoteSwitch
{
    public class LPD433MHzCode
    {
        public LPD433MHzCode(uint value, byte length)
        {
            Value = value;
            Length = length;
        }

        public uint Value { get; }

        public byte Length { get; }
    }
}
