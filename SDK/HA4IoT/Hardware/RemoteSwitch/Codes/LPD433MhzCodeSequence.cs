using System;
using System.Collections.Generic;

namespace HA4IoT.Hardware.RemoteSwitch.Codes
{
    public class Lpd433MhzCodeSequence
    {
        public List<Lpd433MhzCode> Codes { get; } = new List<Lpd433MhzCode>();

        public Lpd433MhzCodeSequence WithCode(Lpd433MhzCode code)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));

            Codes.Add(code);
            return this;
        }
    }
}
