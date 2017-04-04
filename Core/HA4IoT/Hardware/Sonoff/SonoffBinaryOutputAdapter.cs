using System;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.Services;

namespace HA4IoT.Hardware.Sonoff
{
    public class SonoffBinaryOutputAdapter : IBinaryOutputAdapter
    {
        private readonly string _topic;
        private readonly IDeviceMessageBrokerService _deviceMessageBrokerService;

        public SonoffBinaryOutputAdapter(string topic, IDeviceMessageBrokerService deviceMessageBrokerService)
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (deviceMessageBrokerService == null) throw new ArgumentNullException(nameof(deviceMessageBrokerService));

            _topic = topic;
            _deviceMessageBrokerService = deviceMessageBrokerService;
        }

        public void TurnOn(params IHardwareParameter[] parameters)
        {
            _deviceMessageBrokerService.Publish(_topic, "ON", MqttQosLevel.ExactlyOnce);
        }

        public void TurnOff(params IHardwareParameter[] parameters)
        {
            _deviceMessageBrokerService.Publish(_topic, "OFF", MqttQosLevel.ExactlyOnce);
        }
    }
}