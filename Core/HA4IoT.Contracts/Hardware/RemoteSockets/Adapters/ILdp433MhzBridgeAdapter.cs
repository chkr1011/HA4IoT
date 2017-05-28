using System;
using HA4IoT.Contracts.Devices;
using HA4IoT.Contracts.Hardware.RemoteSockets.Codes;

namespace HA4IoT.Contracts.Hardware.RemoteSockets.Adapters
{
    public interface ILdp433MhzBridgeAdapter : IDevice
    {
        event EventHandler<Ldp433MhzCodeReceivedEventArgs> CodeReceived;

        void SendCode(Lpd433MhzCode code);
    }
}
