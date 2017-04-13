namespace HA4IoT.Hardware.RemoteSwitch.Codes
{
    public class Lpd433MhzCode
    {
        public Lpd433MhzCode(uint value, int length, int protocol, int repeats)
        {
            Value = value;
            Length = length;
            Protocol = protocol;
            Repeats = repeats;
        }

        public uint Value { get; }

        public int Length { get; }

        public int Protocol { get; }

        public int Repeats { get; }
    }
}
