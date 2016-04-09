using HA4IoT.Components;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Sensors
{
    public abstract class SensorBase : ComponentBase, ISensor
    {
        private IComponentState _state = new UnknownComponentState();

        protected SensorBase(ComponentId id)
            : base(id)
        {
        }

        public override IComponentState GetState()
        {
            return _state;
        }

        protected void SetState(IComponentState newState)
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