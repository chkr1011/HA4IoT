using HA4IoT.Components;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Sensors
{
    public abstract class SensorBase : ComponentBase, ISensor
    {
        private ComponentState _state = new ComponentState(null);

        protected SensorBase(ComponentId id)
            : base(id)
        {
        }

        public override ComponentState GetState()
        {
            return _state;
        }

        protected void SetState(ComponentState newState)
        {
            if (newState.Equals(_state))
            {
                return;
            }
           
            var oldValue = _state;
            _state = newState;

            OnActiveStateChanged(oldValue, newState);
        }
    }
}