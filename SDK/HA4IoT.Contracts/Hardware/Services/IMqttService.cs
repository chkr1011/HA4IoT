using System;
using HA4IoT.Contracts.Hardware.Mqtt;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Hardware.Services
{
    public interface IMqttService : IService
    {
        event EventHandler<MqttMessageReceivedEventArgs> MessageReceived;

        int Publish(string topic, byte[] message, MqttQosLevel qosLevel);

        void Subscribe(string topicPattern, Action<MqttMessage> callback);
    }
}
