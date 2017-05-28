using System;

namespace HA4IoT.Contracts.Hardware.RemoteSockets.Codes
{
    public class Lpd433MhzCodePair
    {
        public Lpd433MhzCodePair(Lpd433MhzCode onCode, Lpd433MhzCode offCode)
        {
            if (onCode == null) throw new ArgumentNullException(nameof(onCode));
            if (offCode == null) throw new ArgumentNullException(nameof(offCode));

            OnCode = onCode;
            OffCode = offCode;
        }

        public Lpd433MhzCode OnCode { get; }

        public Lpd433MhzCode OffCode { get; }
    }
}
