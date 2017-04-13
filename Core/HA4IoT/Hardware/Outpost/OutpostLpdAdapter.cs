using System;
using System.Text;
using System.Text.RegularExpressions;
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
            _deviceName = deviceName ?? throw new ArgumentNullException(nameof(deviceName));
            _deviceMessageBroker = deviceMessageBroker ?? throw new ArgumentNullException(nameof(deviceMessageBroker));

            _deviceMessageBroker.Subscribe($"HA4IoT/Device/{_deviceName}/Notification/LPD/Received", OnLpdCodeReceived);
        }

        public event EventHandler<Ldp433MhzCodeReceivedEventArgs> CodeReceived;

        public void SendCode(Lpd433MhzCode code)
        {
            // TODO: Add protocol to code class.
            var payload = Encoding.UTF8.GetBytes($"{code.Value},{code.Length},2,{code.Repeats}");
            _deviceMessageBroker.Publish($"HA4IoT/Device/{_deviceName}/Command/LPD/Send", payload, MqttQosLevel.AtMostOnce);
        }

        private void OnLpdCodeReceived(DeviceMessage deviceMessage)
        {
            var message = Encoding.UTF8.GetString(deviceMessage.Payload);
            var regexMatch = Regex.Match(message, "([0-9]+),([0-9]+),([0-9]+)");

            var value = uint.Parse(regexMatch.Groups[1].Value);
            var length = int.Parse(regexMatch.Groups[2].Value);
            var protocol = int.Parse(regexMatch.Groups[3].Value);

            CodeReceived?.Invoke(this, new Ldp433MhzCodeReceivedEventArgs(new Lpd433MhzCode(value, length, protocol, 0)));
        }
    }
}
