using System;
using HA4IoT.Commands;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators.Lamps
{
    public class Lamp : ComponentBase, ILamp
    {
        private readonly IBinaryOutputAdapter _adapter;

        private PowerStateValue _powerState = PowerStateValue.Off;

        public Lamp(string id, IBinaryOutputAdapter adapter)
            : base(id)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            _adapter = adapter;
        }

        public override ComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection()
                .With(new PowerState(_powerState));
        }

        public override ComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection()
                .With(new PowerStateFeature());
        }

        public void ResetState()
        {
            SetStateInternal(PowerStateValue.Off, true);
        }

        public override void ExecuteCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var commandExecutor = new CommandExecutor();
            commandExecutor.Register<TurnOnCommand>(c => SetStateInternal(PowerStateValue.On));
            commandExecutor.Register<TurnOffCommand>(c => SetStateInternal(PowerStateValue.Off));
            commandExecutor.Register<TogglePowerStateCommand>(c => TogglePowerState());
            commandExecutor.Invoke(command);
        }

        private void TogglePowerState()
        {
            SetStateInternal(_powerState == PowerStateValue.Off ? PowerStateValue.On : PowerStateValue.Off);
        }

        private void SetStateInternal(PowerStateValue powerState, bool forceUpdate = false)
        {
            if (!forceUpdate && _powerState == powerState)
            {
                return;
            }

            var oldState = GetState();

            var parameters = forceUpdate ? new IHardwareParameter[] { HardwareParameter.ForceUpdateState } : new IHardwareParameter[0];
            if (powerState == PowerStateValue.On)
            {
                _adapter.TurnOn(parameters);
            }
            else if (powerState == PowerStateValue.Off)
            {
                _adapter.TurnOff(parameters);
            }

            _powerState = powerState;

            var newState = GetState();

            OnStateChanged(oldState, newState);
        }
    }
}