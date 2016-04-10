using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using HA4IoT.Actuators.Animations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Networking;

namespace HA4IoT.Actuators.BinaryStateActuators
{
    public class LogicalBinaryStateActuator : ActuatorBase
    {
        private readonly JsonArray _supportedStatesJson = new JsonArray();
        private readonly IHomeAutomationTimer _timer;

        public LogicalBinaryStateActuator(ComponentId id, IHomeAutomationTimer timer) 
            : base(id)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;

            _supportedStatesJson.Add(BinaryStateId.Off.ToJsonValue());
            _supportedStatesJson.Add(BinaryStateId.On.ToJsonValue());
        }

        public IList<IStateMachine> Actuators { get; } = new List<IStateMachine>();

        public LogicalBinaryStateActuator WithActuator(IStateMachine actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            Actuators.Add(actuator);
            return this;
        }

        public override IComponentState GetState()
        {
            if (Actuators.Any(a => a.GetState().Equals(BinaryStateId.On)))
            {
                return BinaryStateId.On;
            }

            return BinaryStateId.Off;
        }

        public override void SetState(IComponentState state, params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            var animationParameter = parameters.SingleOrDefault(p => p is AnimateParameter) as AnimateParameter;
            if (animationParameter != null)
            {
                Animate(animationParameter, state);
                return;
            }

            foreach (var actuator in Actuators)
            {
                actuator.SetState(state, parameters);
            }

            ////foreach (var actuator in Actuators)
            ////{
            ////    actuator.SetState(state, HardwareParameter.IsPartOfPartialUpdate);
            ////}

            ////bool commit = !parameters.Any(p => p is IsPartOfPartialUpdateParameter);
            ////bool forceUpdate = parameters.Any(p => p is ForceUpdateStateParameter);
            ////if (!commit && !forceUpdate)
            ////{
            ////    return;
            ////}

            ////foreach (var actuator in Actuators)
            ////{
            ////    actuator.SetState(state, parameters);
            ////}
        }

        public void ToggleState(params IHardwareParameter[] parameters)
        {
            if (GetState().Equals(BinaryStateId.Off))
            {
                SetState(BinaryStateId.On, parameters);
            }
            else
            {
                SetState(BinaryStateId.Off, parameters);
            }
        }

        public LogicalBinaryStateActuator ConnectToggleActionWith(IButton button, ButtonPressedDuration pressedDuration = ButtonPressedDuration.Short)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));

            if (pressedDuration == ButtonPressedDuration.Short)
            {
                button.GetPressedShortlyTrigger().Attach(() => ToggleState());
            }
            else if (pressedDuration == ButtonPressedDuration.Long)
            {
                button.GetPressedLongTrigger().Attach(() => ToggleState());
            }
            else
            {
                throw new NotSupportedException();
            }

            return this;
        }

        public override JsonObject ExportConfigurationToJsonObject()
        {
            JsonObject configuration = base.ExportConfigurationToJsonObject();
            
            configuration.SetNamedArray(ComponentConfigurationKey.SupportedStates, _supportedStatesJson);

            return configuration;
        }
        
        private void Animate(AnimateParameter animateParameter, IComponentState newState)
        {
            var directionAnimation = new DirectionAnimation(_timer);
            directionAnimation.WithActuator(this);
            directionAnimation.WithTargetState(newState);

            if (animateParameter.Reverse)
            {
                directionAnimation.WithReversed();
            }

            directionAnimation.Start();
        }
    }
}