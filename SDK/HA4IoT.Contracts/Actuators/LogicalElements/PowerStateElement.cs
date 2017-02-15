using System;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Actuators.LogicalElements
{
    public class PowerStateElement
    {
        private readonly IBinaryOutputComponentAdapter _adapter;

        private PowerStateValue _powerState = PowerStateValue.Off;

        public PowerStateElement(IBinaryOutputComponentAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            _adapter = adapter;

            TogglePowerStateAction = new ActionWrapper(TogglePowerState);
        }

        public event EventHandler<ComponentFeatureStateChangedEventArgs> StateChanged; 

        public IAction TogglePowerStateAction { get; }

        public PowerState GetState()
        {
            return new PowerState(_powerState);
        }

        // TODO: GetFeature();

        public bool TryChangeState(IComponentFeatureState state, params IHardwareParameter[] parameters)
        {
            if (state.Equals(BinaryStateId.Off) || state.Equals(PowerState.Off))
            {
                TurnOffInternal();
                return true;
            }

            if (state.Equals(BinaryStateId.On) || state.Equals(PowerState.On))
            {
                TurnOnInternal();
                return true;
            }

            return false;
        }

        public void ResetState()
        {
            TurnOffInternal(true);
        }

        public bool TryInvokeCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            if (command is TurnOnCommand)
            {
                TurnOnInternal();
                return true;
            }

            if (command is TurnOffCommand)
            {
                TurnOffInternal();
                return true;
            }

            return false;
        }

        public void TogglePowerState()
        {
            if (_powerState == PowerStateValue.Off)
            {
                TurnOnInternal();
            }
            else
            {
                TurnOffInternal();
            }
        }

        private void TurnOffInternal(bool force = false)
        {
            if (!force && _powerState == PowerStateValue.Off)
            {
                return;
            }

            if (force)
            {
                _adapter.TurnOff(HardwareParameter.ForceUpdateState);
            }
            else
            {
                _adapter.TurnOff();
            }

            _powerState = PowerStateValue.Off;
            //StateChanged?.Invoke(this, new ComponentFeatureStateChangedEventArgs(oldState, newState));
        }

        private void TurnOnInternal()
        {
            if (_powerState == PowerStateValue.On)
            {
                return;
            }

            _adapter.TurnOn();
            _powerState = PowerStateValue.On;
            //StateChanged?.Invoke(this, new ComponentFeatureStateChangedEventArgs(PowerState.Off, PowerState.On));
        }
    }
}
