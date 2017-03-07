using System;
using HA4IoT.Hardware.RemoteSwitch.Codes;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public interface ILdp433MhzAdapter
    {
        event EventHandler<Ldp433MhzCodeSequenceReceivedEventArgs> CodeSequenceReceived;

        void SendCodeSequence(Lpd433MhzCodeSequence codeSequence);
    }
}
