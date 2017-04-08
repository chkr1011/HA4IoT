using System;
using System.Text;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Hardware.RemoteSwitch.Codes;

namespace HA4IoT.Hardware.Outpost
{
    public class OutpostLpdAdapter : ILdp433MhzBridgeAdapter
    {
        private readonly string _deviceName;
        private readonly IDeviceMessageBrokerService _deviceMessageBroker;

        public OutpostLpdAdapter(string deviceName, IDeviceMessageBrokerService deviceMessageBroker)
        {
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));
            if (deviceMessageBroker == null) throw new ArgumentNullException(nameof(deviceMessageBroker));

            _deviceName = deviceName;
            _deviceMessageBroker = deviceMessageBroker;
        }
        
        public event EventHandler<Ldp433MhzCodeReceivedEventArgs> CodeReceived;

        public void SendCode(Lpd433MhzCode code)
        {
            // TODO: Add protocol to code class.
            var payload = Encoding.UTF8.GetBytes($"{code.Value},{code.Length},2,{code.Repeats}");
            _deviceMessageBroker.Publish($"HA4IoT/Device/{_deviceName}/Command/LPD/Send", payload, MqttQosLevel.AtMostOnce);
        }
    }
}
