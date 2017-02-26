using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Hardware.Services
{
    public interface IMqttService : IService
    {
        int Publish(string topic, byte[] message, MqttQosLevel qosLevel);
    }
}
