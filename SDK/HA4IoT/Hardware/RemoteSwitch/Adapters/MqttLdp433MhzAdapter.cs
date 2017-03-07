using System;
using HA4IoT.Hardware.RemoteSwitch.Codes;

namespace HA4IoT.Hardware.RemoteSwitch.Adapters
{
    public class MqttLdp433MhzAdapter : ILdp433MhzAdapter
    {
        public event EventHandler<Ldp433MhzCodeSequenceReceivedEventArgs> CodeSequenceReceived;

        public void SendCodeSequence(Lpd433MhzCodeSequence codeSequence)
        {
            throw new System.NotImplementedException();
        }
    }
}
