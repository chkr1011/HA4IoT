namespace HA4IoT.Contracts.Hardware.RemoteSockets.Codes
{
    public sealed class Lpd433MhzCode
    {
        public uint Value { get; set; }

        public int Length { get; set; }

        public int Protocol { get; set; }

        public int Repeats { get; set; }
    }
}
