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
        private readonly ILampAdapter _adapter;

        private PowerStateValue _powerState = PowerStateValue.Off;
        private ColorState _colorState;

        public Lamp(string id, ILampAdapter adapter)
            : base(id)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));

            if (adapter.SupportsColor)
            {
                _colorState = new ColorState();
            }
        }

        public override IComponentFeatureStateCollection GetState()
        {
            var state = new ComponentFeatureStateCollection()
                .With(new PowerState(_powerState));

            if (_adapter.SupportsColor)
            {
                return state.With(_colorState);
            }

            return state;
        }

        public override IComponentFeatureCollection GetFeatures()
        {
            var features = new ComponentFeatureCollection()
                .With(new PowerStateFeature());

            if (_adapter.SupportsColor)
            {
                return features.With(new ColorFeature());
            }

            return features;
        }

        public override void ExecuteCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var commandExecutor = new CommandExecutor();
            commandExecutor.Register<ResetCommand>(c => ResetState());
            commandExecutor.Register<TurnOnCommand>(c => SetStateInternal(PowerStateValue.On, _colorState));
            commandExecutor.Register<TurnOffCommand>(c => SetStateInternal(PowerStateValue.Off, _colorState));
            commandExecutor.Register<TogglePowerStateCommand>(c => TogglePowerState());

            if (_adapter.SupportsColor)
            {
                commandExecutor.Register<SetColorCommand>(c => SetStateInternal(_powerState, GenerateColorState(c)));
            }

            commandExecutor.Execute(command);
        }

        public void ResetState()
        {
            SetStateInternal(PowerStateValue.Off, _colorState, true);
        }

        private void TogglePowerState()
        {
            SetStateInternal(_powerState == PowerStateValue.Off ? PowerStateValue.On : PowerStateValue.Off, _colorState);
        }

        private void SetStateInternal(PowerStateValue powerState, ColorState colorState, bool forceUpdate = false)
        {
            if (colorState == null)
            {
                if (!forceUpdate && _powerState == powerState)
                {
                    return;
                }
            }
            else if (!forceUpdate && _powerState == powerState && colorState.Equals(_colorState))
            {
                return;
            }

            var oldState = GetState();

            var parameters = forceUpdate ? new IHardwareParameter[] { HardwareParameter.ForceUpdateState } : new IHardwareParameter[0];
            if (powerState == PowerStateValue.On)
            {
                _adapter.SetState(AdapterPowerState.On, GenerateAdapterColor(colorState), parameters);
            }
            else if (powerState == PowerStateValue.Off)
            {
                _adapter.SetState(AdapterPowerState.Off, GenerateAdapterColor(colorState), parameters);
            }

            _powerState = powerState;
            _colorState = colorState;

            OnStateChanged(oldState);
        }

        private ColorState GenerateColorState(SetColorCommand setColorCommand)
        {
            return new ColorState
            {
                Hue = setColorCommand.Hue,
                Saturation = setColorCommand.Saturation,
                Value = setColorCommand.Value
            };
        }

        private AdapterColor GenerateAdapterColor(ColorState colorState)
        {
            if (!_adapter.SupportsColor)
            {
                return null;
            }

            double r, g, b;
            ColorConverter.ConvertHsvToRgb(colorState.Hue, colorState.Saturation, colorState.Value, out r, out g, out b);

            var maxValue = Math.Pow(2, _adapter.ColorResolutionBits);
            r = maxValue * r;
            g = maxValue * g;
            b = maxValue * b;

            return new AdapterColor
            {
                Red = (int)r,
                Green = (int)g,
                Blue = (int)b
            };
        }
    }
}