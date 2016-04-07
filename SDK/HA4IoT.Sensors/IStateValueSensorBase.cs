using System;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Sensors
{
    public abstract class StateValueSensorBase : SensorBase, IStateValueSensor
    {
        private StateId _activeState;

        protected StateValueSensorBase(ComponentId id) 
            : base(id)
        {
        }

        public event EventHandler<StateChangedEventArgs> ActiveStateChanged;

        public StateId GetActiveState()
        {
            return _activeState;
        }

        protected void SetActiveState(StateId newState)
        {
            var oldState = _activeState;
            if (newState.Equals(oldState))
            {
                return;
            }

            _activeState = newState;
            OnActiveStateChanged(oldState);
        }

        protected void OnActiveStateChanged(StateId oldState)
        {
            StateId newState = GetActiveState();
            Log.Info($"Actuator '{Id}' updated state from '{oldState}' to '{newState}'");

            ApiController?.NotifyStateChanged(this);
            ActiveStateChanged?.Invoke(this, new StateChangedEventArgs(oldState, newState));
        }
    }
}
