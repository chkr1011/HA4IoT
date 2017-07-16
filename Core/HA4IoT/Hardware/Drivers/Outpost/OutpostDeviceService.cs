using System;
using HA4IoT.Components.Adapters.MqttBased;
using HA4IoT.Contracts.Components.Adapters;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Devices;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Hardware.Outpost;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Scheduling;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Hardware.Drivers.Outpost
{
    public class OutpostDeviceService : ServiceBase
    {
        private readonly II2CBusService _i2CBusService;
        private readonly ISchedulerService _schedulerService;
        private readonly IDeviceMessageBrokerService _deviceMessageBroker;
        private readonly ILogService _logService;

        public OutpostDeviceService(
            IDeviceRegistryService deviceRegistryService,
            IDeviceMessageBrokerService deviceMessageBroker, 
            II2CBusService i2CBusService,
            ISchedulerService schedulerService,
            ILogService logService)
        {
            if (deviceRegistryService == null) throw new ArgumentNullException(nameof(deviceRegistryService));
            _i2CBusService = i2CBusService ?? throw new ArgumentNullException(nameof(i2CBusService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            _deviceMessageBroker = deviceMessageBroker ?? throw new ArgumentNullException(nameof(deviceMessageBroker));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            deviceRegistryService.RegisterDeviceFactory(new OutpostDeviceFactory(this));
        }

        public INumericSensorAdapter CreateDhtTemperatureSensorAdapter(string deviceName)
        {
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));

            var topic = OutpostTopicBuilder.BuildNotificationTopic(deviceName, "DHT/Temperature");
            return new MqttBasedNumericSensorAdapter(topic, _deviceMessageBroker, _logService);
        }

        public INumericSensorAdapter CreateDhtHumiditySensorAdapter(string deviceName)
        {
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));

            var topic = OutpostTopicBuilder.BuildNotificationTopic(deviceName, "DHT/Humidity");
            return new MqttBasedNumericSensorAdapter(topic, _deviceMessageBroker, _logService);
        }

        public OutpostRgbAdapter CreateRgbStripAdapter(string deviceName)
        {
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));

            return new OutpostRgbAdapter(deviceName, _deviceMessageBroker);
        }

        public OutpostLpdBridgeAdapter CreateLpdBridgeAdapter(string id, string deviceName)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (deviceName == null) throw new ArgumentNullException(nameof(deviceName));

            return new OutpostLpdBridgeAdapter(id, deviceName, _deviceMessageBroker);
        }

        public IDevice CreateI2CHardwareBridge(string id, int address)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            return new I2CHardwareBridge.I2CHardwareBridge(id, new I2CSlaveAddress(address), _i2CBusService, _schedulerService);
        }
    }
}

