namespace HA4IoT.Contracts.Actuators
{
    public interface IBinaryStateEndpoint
    {
        void TurnOn(params IHardwareParameter[] parameters);

        void TurnOff(params IHardwareParameter[] parameters);
    }
}
