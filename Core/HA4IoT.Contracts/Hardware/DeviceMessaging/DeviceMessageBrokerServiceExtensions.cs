using System;
using System.Text;
using HA4IoT.Contracts.Hardware.Mqtt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Hardware.DeviceMessaging
{
    public static class DeviceMessageBrokerServiceExtensions
    {
        public static void Publish(this IDeviceMessageBrokerService deviceMessageBrokerService, string topic, JObject payload, MqttQosLevel qosLevel, bool retain)
        {
            if (deviceMessageBrokerService == null) throw new ArgumentNullException(nameof(deviceMessageBrokerService));
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            deviceMessageBrokerService.Publish(topic, payload.ToString(Formatting.None), qosLevel, retain);
        }

        public static void Publish(this IDeviceMessageBrokerService deviceMessageBrokerService, string topic, string payload, MqttQosLevel qosLevel, bool retain)
        {
            if (deviceMessageBrokerService == null) throw new ArgumentNullException(nameof(deviceMessageBrokerService));
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            deviceMessageBrokerService.Publish(topic, Encoding.UTF8.GetBytes(payload), qosLevel, retain);
        }

        public static void Publish(this IDeviceMessageBrokerService deviceMessageBrokerService, DeviceMessage deviceMessage)
        {
            if (deviceMessageBrokerService == null) throw new ArgumentNullException(nameof(deviceMessageBrokerService));
            if (deviceMessage == null) throw new ArgumentNullException(nameof(deviceMessage));

            deviceMessageBrokerService.Publish(deviceMessage.Topic, deviceMessage.Payload, deviceMessage.QosLevel, deviceMessage.Retain);
        }
    }
}