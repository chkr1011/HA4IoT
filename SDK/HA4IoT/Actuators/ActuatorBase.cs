using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators
{
    public abstract class ActuatorBase : ComponentBase, IActuator
    {
        protected ActuatorBase(ComponentId id) 
            : base(id)
        {
        }

        public abstract void SetState(ComponentState state, params IHardwareParameter[] parameters);

        public abstract void ResetState();
    }
}