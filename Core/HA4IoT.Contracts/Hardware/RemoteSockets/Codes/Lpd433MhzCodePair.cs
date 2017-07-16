using System;

namespace HA4IoT.Contracts.Hardware.RemoteSockets.Codes
{
    public sealed class Lpd433MhzCodePair
    {
        public Lpd433MhzCodePair(Lpd433MhzCode onCode, Lpd433MhzCode offCode)
        {
            OnCode = onCode ?? throw new ArgumentNullException(nameof(onCode));
            OffCode = offCode ?? throw new ArgumentNullException(nameof(offCode));
        }

        public Lpd433MhzCode OnCode { get; }

        public Lpd433MhzCode OffCode { get; }
    }
}
