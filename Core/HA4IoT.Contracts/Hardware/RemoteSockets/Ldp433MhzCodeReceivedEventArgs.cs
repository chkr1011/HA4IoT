using System;
using HA4IoT.Contracts.Hardware.RemoteSockets.Codes;

namespace HA4IoT.Contracts.Hardware.RemoteSockets
{
    public sealed class Ldp433MhzCodeReceivedEventArgs : EventArgs
    {
        public Ldp433MhzCodeReceivedEventArgs(Lpd433MhzCode code)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
        }

        public Lpd433MhzCode Code { get; }
    }
}
