namespace HA4IoT.Contracts.Hardware.Mqtt
{
    public enum MqttQosLevel
    {
        AtMostOnce = 0,
        AtLeastOnce = 1,
        ExactlyOnce = 2,
        GrantedFailure = 128,
    }
}
