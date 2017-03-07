using System;

namespace HA4IoT.Contracts.Hardware.Mqtt
{
    public class MqttMessageReceivedEventArgs
    {
        public MqttMessageReceivedEventArgs(MqttMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            Message = message;
        }

        public MqttMessage Message { get; }
    }
}
