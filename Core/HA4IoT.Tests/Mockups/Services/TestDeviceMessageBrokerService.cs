using System;
using HA4IoT.Contracts.Hardware.DeviceMessaging;
using HA4IoT.Contracts.Hardware.Services;

namespace HA4IoT.Tests.Mockups.Services
{
    public class TestDeviceMessageBrokerService : IDeviceMessageBrokerService
    {
        public void Startup()
        {
        }

        public event EventHandler<DeviceMessageReceivedEventArgs> MessageReceived;

        public void Publish(string topic, byte[] payload, MqttQosLevel qosLevel)
        {
            var deviceMessage = new DeviceMessage
            {
                Topic = topic,
                Payload = payload,
                QosLevel = qosLevel
            };

            MessageReceived?.Invoke(this, new DeviceMessageReceivedEventArgs(deviceMessage));
        }

        public void Subscribe(string topicPattern, Action<DeviceMessage> callback)
        {
        }
    }
}
