namespace HA4IoT.Hardware.RemoteSwitch
{
    public class LPD433MHzCode
    {
        public LPD433MHzCode(uint value, byte length, byte repeats)
        {
            Value = value;
            Length = length;
            Repeats = repeats;
        }

        public uint Value { get; }

        public byte Length { get; }

        public byte Repeats { get; }
    }
}
