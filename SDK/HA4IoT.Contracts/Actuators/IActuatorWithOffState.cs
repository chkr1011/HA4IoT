using HA4IoT.Contracts.Actions;

namespace HA4IoT.Contracts.Actuators
{
    public interface IActuatorWithOffState
    {
        void TurnOff(params IHardwareParameter[] parameters);

        IHomeAutomationAction GetTurnOffAction();
    }
}
