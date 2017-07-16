using System;
using System.Text;
using HA4IoT.Contracts.Components.Adapters;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Components.Adapters.MqttBased
{
    public class MqttBasedNumericSensorAdapter : INumericSensorAdapter
    {
        private readonly string _topic;
        private readonly ILogger _log;

        public MqttBasedNumericSensorAdapter(string topic, IDeviceMessageBrokerService deviceMessageBrokerService, ILogService logService)
        {
            if (deviceMessageBrokerService == null) throw new ArgumentNullException(nameof(deviceMessageBrokerService));

            _log = logService?.CreatePublisher(nameof(MqttBasedNumericSensorAdapter)) ?? throw new ArgumentNullException(nameof(logService));

            _topic = topic ?? throw new ArgumentNullException(nameof(topic));
            deviceMessageBrokerService.Subscribe(topic, ForwardValue);
        }

        public event EventHandler<NumericSensorAdapterValueChangedEventArgs> ValueChanged;

        public void Refresh()
        {
        }

        private void ForwardValue(DeviceMessage deviceMessage)
        {
            var payload = Encoding.UTF8.GetString(deviceMessage.Payload);

            float value;
            if (!float.TryParse(payload, out value))
            {
                _log.Warning($"Unable to parse MQTT payload '{payload}' of topic '{_topic}' to numeric value.");
                return;
            }

            ValueChanged?.Invoke(this, new NumericSensorAdapterValueChangedEventArgs(value));
        }
    }
}
