using System;
using System.Threading.Tasks;
using HA4IoT.Contracts.Components.Adapters;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.Mqtt;
using HA4IoT.Contracts.Hardware.Outpost;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Hardware.Drivers.Outpost
{
    public class OutpostRgbAdapter : IBinaryOutputAdapter, ILampAdapter
    {
        private readonly string _deviceName;
        private readonly IDeviceMessageBrokerService _deviceMessageBroker;

        public OutpostRgbAdapter(string deviceName, IDeviceMessageBrokerService deviceMessageBroker)
        {
            _deviceName = deviceName ?? throw new ArgumentNullException(nameof(deviceName));
            _deviceMessageBroker = deviceMessageBroker ?? throw new ArgumentNullException(nameof(deviceMessageBroker));
        }

        public bool SupportsColor => true;

        public int ColorResolutionBits => 10;

        public Task SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters)
        {
            return SetState(powerState, null, parameters);
        }

        public Task SetState(AdapterPowerState powerState, AdapterColor color, params IHardwareParameter[] hardwareParameters)
        {
            if (!SupportsColor && color != null)
            {
                throw new InvalidOperationException("Color is not supported by adapter.");
            }

            var r = 0;
            var g = 0;
            var b = 0;

            if (powerState == AdapterPowerState.On && color == null)
            {
                r = 1023;
                g = 1023;
                b = 1023;
            }
            else if (powerState == AdapterPowerState.On && color != null)
            {
                r = color.Red;
                g = color.Green;
                b = color.Blue;
            }

            var topic = OutpostTopicBuilder.BuildCommandTopic(_deviceName, "RGB/Set");
            var json = new JObject
            {
                ["r"] = r,
                ["g"] = g,
                ["b"] = b
            };

            _deviceMessageBroker.Publish(topic, json, MqttQosLevel.AtMostOnce);
            return Task.FromResult(0);
        }
    }
}
