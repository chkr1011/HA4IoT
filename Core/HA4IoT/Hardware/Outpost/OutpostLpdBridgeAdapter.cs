using System;
using System.Text;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.Mqtt;
using HA4IoT.Contracts.Hardware.Outpost;
using HA4IoT.Contracts.Hardware.RemoteSockets;
using HA4IoT.Contracts.Hardware.RemoteSockets.Adapters;
using HA4IoT.Contracts.Hardware.RemoteSockets.Codes;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Hardware.Outpost
{
    public class OutpostLpdBridgeAdapter : ILdp433MhzBridgeAdapter
    {
        private readonly string _deviceName;
        private readonly IDeviceMessageBrokerService _deviceMessageBroker;

        public OutpostLpdBridgeAdapter(string id, string deviceName, IDeviceMessageBrokerService deviceMessageBroker)
        {
            _deviceName = deviceName ?? throw new ArgumentNullException(nameof(deviceName));
            _deviceMessageBroker = deviceMessageBroker ?? throw new ArgumentNullException(nameof(deviceMessageBroker));

            Id = id ?? throw new ArgumentNullException(nameof(id));
            _deviceMessageBroker.Subscribe(OutpostTopicBuilder.BuildNotificationTopic(deviceName, "LPD/Received"), OnLpdCodeReceived);
        }

        public string Id { get; }

        public event EventHandler<Ldp433MhzCodeReceivedEventArgs> CodeReceived;

        public void SendCode(Lpd433MhzCode code)
        {
            var topic = OutpostTopicBuilder.BuildCommandTopic(_deviceName, "LPD/Send");
            var json = new JObject
            {
                ["value"] = code.Value,
                ["length"] = code.Length,
                ["protocol"] = code.Protocol,
                ["repeats"] = code.Repeats
            };

            _deviceMessageBroker.Publish(topic, json, MqttQosLevel.AtMostOnce);
        }

        private void OnLpdCodeReceived(DeviceMessage deviceMessage)
        {
            var json = JObject.Parse(Encoding.UTF8.GetString(deviceMessage.Payload));
            var value = json["value"].Value<uint>();
            var length = json["length"].Value<int>();
            var protocol = json["protocol"].Value<int>();

            CodeReceived?.Invoke(this, new Ldp433MhzCodeReceivedEventArgs(new Lpd433MhzCode(value, length, protocol, 0)));
        }
    }
}
