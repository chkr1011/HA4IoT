using System;
using System.Text;
using HA4IoT.Contracts.Hardware.Outpost;
using HA4IoT.Contracts.Hardware.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Hardware.DeviceMessaging
{
    public static class DeviceMessageBrokerServiceExtensions
    {
        public static void Publish(this IDeviceMessageBrokerService deviceMessageBrokerService, string topic, JObject payload, MqttQosLevel qosLevel)
        {
            if (deviceMessageBrokerService == null) throw new ArgumentNullException(nameof(deviceMessageBrokerService));
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            deviceMessageBrokerService.Publish(topic, payload.ToString(Formatting.None), qosLevel);
        }

        public static void Publish(this IDeviceMessageBrokerService deviceMessageBrokerService, string topic, string payload, MqttQosLevel qosLevel)
        {
            if (deviceMessageBrokerService == null) throw new ArgumentNullException(nameof(deviceMessageBrokerService));
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            deviceMessageBrokerService.Publish(topic, Encoding.UTF8.GetBytes(payload), qosLevel);
        }

        public static void Publish(this IDeviceMessageBrokerService deviceMessageBrokerService, DeviceMessage deviceMessage)
        {
            if (deviceMessageBrokerService == null) throw new ArgumentNullException(nameof(deviceMessageBrokerService));
            if (deviceMessage == null) throw new ArgumentNullException(nameof(deviceMessage));

            deviceMessageBrokerService.Publish(deviceMessage.Topic, deviceMessage.Payload, deviceMessage.QosLevel);
        }

        public static void PublishDeviceMessage(this IDeviceMessageBrokerService deviceMessageBrokerService, string deviceId, string notificationId, string payload)
        {
            if (deviceMessageBrokerService == null) throw new ArgumentNullException(nameof(deviceMessageBrokerService));
            if (deviceId == null) throw new ArgumentNullException(nameof(deviceId));
            if (notificationId == null) throw new ArgumentNullException(nameof(notificationId));

            var topic =  OutpostTopicBuilder.BuildNotificationTopic(deviceId, notificationId);
            deviceMessageBrokerService.Publish(topic, Encoding.UTF8.GetBytes(payload), MqttQosLevel.AtMostOnce);
        }
    }
}