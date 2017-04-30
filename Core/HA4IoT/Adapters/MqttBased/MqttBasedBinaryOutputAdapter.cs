using System;
using System.Text;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Adapters.MqttBased
{
    public class MqttBasedBinaryOutputAdapter : IBinaryOutputAdapter
    {
        private readonly IDeviceMessageBrokerService _deviceMessageBrokerService;
        private readonly string _topic;
        private readonly ILogger _log;

        public MqttBasedBinaryOutputAdapter(string topic, IDeviceMessageBrokerService deviceMessageBrokerService, ILogService logService)
        {
            _deviceMessageBrokerService = deviceMessageBrokerService ?? throw new ArgumentNullException(nameof(deviceMessageBrokerService));
            _log = logService?.CreatePublisher(nameof(MqttBasedButtonAdapter)) ?? throw new ArgumentNullException(nameof(logService));

            _topic = topic ?? throw new ArgumentNullException(nameof(topic));
        }

        public void SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters)
        {
            _deviceMessageBrokerService.Publish(_topic, Encoding.UTF8.GetBytes(powerState.ToString()), MqttQosLevel.AtMostOnce);
        }
    }
}
