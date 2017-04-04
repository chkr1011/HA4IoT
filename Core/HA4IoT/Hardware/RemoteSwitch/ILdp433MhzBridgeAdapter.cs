using System;
using HA4IoT.Hardware.RemoteSwitch.Codes;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public interface ILdp433MhzBridgeAdapter
    {
        event EventHandler<Ldp433MhzCodeReceivedEventArgs> CodeReceived;

        void SendCode(Lpd433MhzCode code);
    }
}
