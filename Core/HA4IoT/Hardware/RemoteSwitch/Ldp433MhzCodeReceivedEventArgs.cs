using System;
using HA4IoT.Hardware.RemoteSwitch.Codes;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public class Ldp433MhzCodeReceivedEventArgs : EventArgs
    {
        public Ldp433MhzCodeReceivedEventArgs(Lpd433MhzCode code)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));

            Code = code;
        }

        public Lpd433MhzCode Code { get; }
    }
}
