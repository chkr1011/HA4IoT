using System;
using System.Text;
using HA4IoT.Contracts.Components.Adapters;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Components.Adapters.MqttBased
{
    public class MqttBasedButtonAdapter : IButtonAdapter
    {
        private readonly ILogger _log;

        public MqttBasedButtonAdapter(string topic, IDeviceMessageBrokerService deviceMessageBrokerService, ILogService logService)
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (deviceMessageBrokerService == null) throw new ArgumentNullException(nameof(deviceMessageBrokerService));

            _log = logService?.CreatePublisher(nameof(MqttBasedButtonAdapter)) ?? throw new ArgumentNullException(nameof(logService));

            deviceMessageBrokerService.Subscribe(topic, ForwardState);
        }

        public event EventHandler<ButtonAdapterStateChangedEventArgs> StateChanged;
        
        private void ForwardState(DeviceMessage deviceMessage)
        {
            var payload = Encoding.UTF8.GetString(deviceMessage.Payload);

            AdapterButtonState state;
            if (!Enum.TryParse(payload, true, out state))
            {
                _log.Warning($"Unable to parse MQTT payload '{payload}' to valid button state.");
                return;
            }

            if (state == AdapterButtonState.Pressed)
            {
                StateChanged?.Invoke(this, new ButtonAdapterStateChangedEventArgs(AdapterButtonState.Pressed));
            }
            else if (state == AdapterButtonState.Released)
            {
                StateChanged?.Invoke(this, new ButtonAdapterStateChangedEventArgs(AdapterButtonState.Released));
            }
        }
    }
}
