using System;
using System.Threading.Tasks;
using HA4IoT.Contracts.Components.Adapters;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.Mqtt;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Components.Adapters.MqttBased
{
    public sealed class RgbDeviceAdapter : ILampAdapter
    {
        private readonly IDeviceMessageBrokerService _messageBroker;
        private readonly string _topic;

        public RgbDeviceAdapter(string topic, IDeviceMessageBrokerService messageBroker)
        {
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));

            _topic = topic;
        }

        public bool SupportsColor { get; } = true;
        public int ColorResolutionBits { get; } = 10;

        public Task SetState(AdapterPowerState powerState, AdapterColor color, params IHardwareParameter[] hardwareParameters)
        {
            var r = 0;
            var g = 0;
            var b = 0;

            if (powerState == AdapterPowerState.On && color != null)
            {
                r = color.Red;
                g = color.Green;
                b = color.Blue;
            }

            if (powerState == AdapterPowerState.On && color == null)
            {
                r = 1023;
                g = 1023;
                b = 1023;
            }

            var value = new JObject
            {
                ["r"] = r,
                ["g"] = g,
                ["b"] = b
            };

            _messageBroker.Publish(_topic, value, MqttQosLevel.AtMostOnce, true);
            return Task.CompletedTask;
        }
    }
}
