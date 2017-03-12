using System;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Hardware.DeviceMessaging
{
    public interface IDeviceMessageBrokerService : IService
    {
        event EventHandler<DeviceMessageReceivedEventArgs> MessageReceived;

        void Publish(string topic, byte[] payload, MqttQosLevel qosLevel);

        void Subscribe(string topicPattern, Action<DeviceMessage> callback);
    }
}
