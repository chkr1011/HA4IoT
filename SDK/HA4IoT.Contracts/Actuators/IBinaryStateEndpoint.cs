namespace HA4IoT.Contracts.Actuators
{
    public interface IBinaryStateEndpoint
    {
        void TurnOn(params IParameter[] parameters);

        void TurnOff(params IParameter[] parameters);
    }
}
