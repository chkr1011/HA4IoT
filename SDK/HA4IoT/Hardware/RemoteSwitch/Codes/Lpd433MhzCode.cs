namespace HA4IoT.Hardware.RemoteSwitch.Codes
{
    public class Lpd433MhzCode
    {
        public Lpd433MhzCode(uint value, byte length, byte repeats)
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
