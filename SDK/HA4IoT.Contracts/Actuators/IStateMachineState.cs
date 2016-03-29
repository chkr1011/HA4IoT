using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Actuators
{
    public interface IStateMachineState
    {
        StateMachineStateId Id { get; }

        void Activate(params IHardwareParameter[] parameters);

        void Deactivate(params IHardwareParameter[] parameters);
    }
}
