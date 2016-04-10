using System.Collections.Generic;
using Windows.Data.Json;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public abstract class ActuatorBase : ComponentBase, IActuator
    {
        protected ActuatorBase(ComponentId id) 
            : base(id)
        {
        }

        public abstract void SetState(IComponentState state, params IHardwareParameter[] parameters);
    }
}