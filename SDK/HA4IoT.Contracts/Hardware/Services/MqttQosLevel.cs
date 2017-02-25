namespace HA4IoT.Contracts.Hardware.Services
{
    public enum MqttQosLevel
    {
        At_Most_Once = 0,
        At_Least_Once = 1,
        Exactly_Once = 2,
        Granted_Failure = 128,
    }
}
