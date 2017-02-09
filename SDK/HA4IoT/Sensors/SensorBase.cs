using HA4IoT.Components;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Sensors
{
    public abstract class SensorBase : ComponentBase, ISensor
    {
        private GenericComponentState _state = new GenericComponentState(null);

        protected SensorBase(ComponentId id)
            : base(id)
        {
        }

        public override ComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection().With(_state);
        }

        protected void SetState(GenericComponentState newState)
        {
            if (newState.Equals(_state))
            {
                return;
            }
           
            var oldValue = _state;
            _state = newState;

            OnStateChanged(oldValue, newState);
        }
    }
}