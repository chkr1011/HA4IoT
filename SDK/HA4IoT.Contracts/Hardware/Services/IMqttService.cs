using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Hardware.Services
{
    public interface IMqttService : IService
    {
        ushort Publish(string topic, byte[] message, MqttQosLevel qosLevel = MqttQosLevel.At_Least_Once);
    }
}
