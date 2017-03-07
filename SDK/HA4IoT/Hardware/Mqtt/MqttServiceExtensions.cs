using System;
using System.Text;
using HA4IoT.Contracts.Hardware.Mqtt;
using HA4IoT.Contracts.Hardware.Services;

namespace HA4IoT.Hardware.Mqtt
{
    public static class MqttServiceExtensions
    {
        public static int Publish(this IMqttService mqttService, string topic, string message, MqttQosLevel qosLevel)
        {
            if (mqttService == null) throw new ArgumentNullException(nameof(mqttService));
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (message == null) throw new ArgumentNullException(nameof(message));

            return mqttService.Publish(topic, Encoding.UTF8.GetBytes(message), qosLevel);
        }

        public static void Publish(this IMqttService mqttService, MqttMessage mqttMessageParameters)
        {
            if (mqttService == null) throw new ArgumentNullException(nameof(mqttService));
            if (mqttMessageParameters == null) throw new ArgumentNullException(nameof(mqttMessageParameters));

            mqttService.Publish(mqttMessageParameters.Topic, mqttMessageParameters.Message, mqttMessageParameters.QosLevel);
        }
    }
}
