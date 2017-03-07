namespace HA4IoT.Contracts.Hardware.Services
{
    public enum MqttQosLevel
    {
        AtMostOnce = 0,
        AtLeastOnce = 1,
        ExactlyOnce = 2,
        GrantedFailure = 128,
    }
}
