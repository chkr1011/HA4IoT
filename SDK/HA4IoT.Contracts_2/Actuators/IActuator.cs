using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Actuators
{
    public interface IActuator : IComponent
    {
        void SetState(ComponentState state, params IHardwareParameter[] parameters);

        void ResetState();
    }
}
