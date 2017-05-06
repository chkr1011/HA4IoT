using System;
using HA4IoT.Adapters.MqttBased;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.Outpost;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Hardware.Outpost
{
    public class OutpostDeviceService : ServiceBase
    {
        private readonly ILogService _logService;
        private readonly IDeviceMessageBrokerService _deviceMessageBroker;

        public OutpostDeviceService(IDeviceMessageBrokerService deviceMessageBroker, ILogService logService)
        {
            _deviceMessageBroker = deviceMessageBroker ?? throw new ArgumentNullException(nameof(deviceMessageBroker));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public INumericSensorAdapter GetDhtTemperatureSensorAdapter(string deviceName)
        {
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));

            var topic = OutpostTopicBuilder.BuildNotificationTopic(deviceName, "DHT/Temperature");
            return new MqttBasedNumericSensorAdapter(topic, _deviceMessageBroker, _logService);
        }

        public INumericSensorAdapter GetDhtHumiditySensorAdapter(string deviceName)
        {
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));

            var topic = OutpostTopicBuilder.BuildNotificationTopic(deviceName, "DHT/Humidity");
            return new MqttBasedNumericSensorAdapter(topic, _deviceMessageBroker, _logService);
        }

        public OutpostRgbAdapter GetRgbStripAdapter(string deviceName)
        {
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));

            return new OutpostRgbAdapter(deviceName, _deviceMessageBroker);
        }

        public OutpostLpdBridgeAdapter GetLpdBridgeAdapter(string deviceName)
        {
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));

            return new OutpostLpdBridgeAdapter(deviceName, _deviceMessageBroker);
        }
    }
}
