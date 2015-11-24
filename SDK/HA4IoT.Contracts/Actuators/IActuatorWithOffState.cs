namespace HA4IoT.Contracts.Actuators
{
    public interface IActuatorWithOffState
    {
        void TurnOff(params IParameter[] parameters);
    }
}
