using System;
using HA4IoT.Contracts.Hardware.Mqtt;
using HA4IoT.Contracts.Hardware.Services;

namespace HA4IoT.Hardware.Mqtt
{
    public static class MqttServiceExtensions
    {
        public static void Publish(this IMqttService mqttService, PublishMqttMessageParameter mqttMessageParameters)
        {
            if (mqttService == null) throw new ArgumentNullException(nameof(mqttService));
            if (mqttMessageParameters == null) throw new ArgumentNullException(nameof(mqttMessageParameters));

            mqttService.Publish(mqttMessageParameters.Topic, mqttMessageParameters.Message, mqttMessageParameters.QosLevel);
        }
    }
}
