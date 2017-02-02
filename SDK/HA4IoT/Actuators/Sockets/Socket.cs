using System;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators.Sockets
{
    public class Socket : ComponentBase, ISocket
    {
        private readonly IBinaryOutputComponentAdapter _adapter;

        private PowerStateValue _powerState = PowerStateValue.Off;

        public Socket(ComponentId id, IBinaryOutputComponentAdapter adapter)
            : base(id)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            _adapter = adapter;

            TogglePowerStateAction = new ActionWrapper(TogglePowerState);
        }

        public IAction TogglePowerStateAction { get; }

        public override ComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection()
                .WithState(new PowerState(_powerState));
        }

        public override ComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection();
        }

        public void ChangeState(IComponentFeatureState state, params IHardwareParameter[] parameters)
        {
            if (state.Equals(BinaryStateId.Off) || state.Equals(PowerState.Off))
            {
                TurnOffInternal();
            }
            else if (state.Equals(BinaryStateId.On) || state.Equals(PowerState.On))
            {
                TurnOnInternal();
            }
            else
            {
                throw new ComponentFeatureStateNotSupportedException(state);
            }
        }

        public void ResetState()
        {
            _adapter.TurnOff(HardwareParameter.ForceUpdateState);
            _powerState = PowerStateValue.Off;

            OnStateChanged(null, PowerState.Off);
        }

        public override void InvokeCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            if (command is TurnOnCommand)
            {
                TurnOnInternal();
            }
            else if (command is TurnOffCommand)
            {
                TurnOffInternal();
            }
            else
            {
                throw new CommandNotSupportedException(command);
            }
        }

        private void TogglePowerState()
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

        private void TurnOffInternal()
        {
            if (_powerState == PowerStateValue.Off)
            {
                return;
            }

            _adapter.TurnOff();
            _powerState = PowerStateValue.Off;
            OnStateChanged(PowerState.On, PowerState.Off);
        }

        private void TurnOnInternal()
        {
            if (_powerState == PowerStateValue.On)
            {
                return;
            }

            _adapter.TurnOn();
            _powerState = PowerStateValue.On;
            OnStateChanged(PowerState.Off, PowerState.On);
        }
    }
}