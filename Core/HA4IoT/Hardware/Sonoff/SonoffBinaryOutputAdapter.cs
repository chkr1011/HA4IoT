using System;
using System.Threading.Tasks;
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
            _topic = topic ?? throw new ArgumentNullException(nameof(topic));
            _deviceMessageBrokerService = deviceMessageBrokerService ?? throw new ArgumentNullException(nameof(deviceMessageBrokerService));
        }

        public Task SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters)
        {
            _deviceMessageBrokerService.Publish(_topic, powerState == AdapterPowerState.On ? "ON" : "OFF", MqttQosLevel.ExactlyOnce);
            return Task.FromResult(0);
        }
    }
}