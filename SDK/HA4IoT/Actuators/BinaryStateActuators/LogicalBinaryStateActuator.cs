using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Actuators.BinaryStateActuators
{
    public class LogicalBinaryStateActuator : ComponentBase, IStateMachine
    {
        private readonly ITimerService _timerService;

        private GenericComponentState _state = new GenericComponentState(null);

        public LogicalBinaryStateActuator(string id, ITimerService timerService) 
            : base(id)
        {
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));

            _timerService = timerService;
        }

        public IList<IComponent> Actuators { get; } = new List<IComponent>();

        public LogicalBinaryStateActuator WithActuator(IComponent actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            actuator.StateChanged += (s, e) =>
            {
                var oldState = _state;
                _state = GetStateInternal();

                if (oldState.Equals(_state))
                {
                    return;
                }

               // OnStateChanged(oldState, _state);
            };

            Actuators.Add(actuator);
            _state = GetStateInternal();

            return this;
        }

        public override ComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection().With(_state);
        }

        public override ComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection();
        }

        public override void InvokeCommand(ICommand command)
        {
        }

        public void ResetState()
        {
            ChangeState(BinaryStateId.Off, new ForceUpdateStateParameter());
        }

        public void ChangeState(IComponentFeatureState state, params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            ////var animationParameter = parameters.SingleOrDefault(p => p is AnimateParameter) as AnimateParameter;
            ////if (animationParameter != null)
            ////{
            ////    Animate(animationParameter, state);
            ////    return;
            ////}

            foreach (var actuator in Actuators)
            {
                // TODO: Fix!
                //actuator.ChangeState(state, parameters);
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
            if (GetState().Has(PowerState.Off))
            {
                ChangeState(BinaryStateId.On, parameters);
            }
            else
            {
                ChangeState(BinaryStateId.Off, parameters);
            }
        }

        public LogicalBinaryStateActuator ConnectToggleActionWith(IButton button, ButtonPressedDuration pressedDuration = ButtonPressedDuration.Short)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));

            if (pressedDuration == ButtonPressedDuration.Short)
            {
                button.PressedShortlyTrigger.Attach(() => ToggleState());
            }
            else if (pressedDuration == ButtonPressedDuration.Long)
            {
                button.PressedLongTrigger.Attach(() => ToggleState());
            }
            else
            {
                throw new NotSupportedException();
            }

            return this;
        }

        public override IList<GenericComponentState> GetSupportedStates()
        {
            return new List<GenericComponentState> {BinaryStateId.Off, BinaryStateId.On};
        }

        private GenericComponentState GetStateInternal()
        {
            if (Actuators.Any(a => a.GetState().Has(PowerState.On)))
            {
                return BinaryStateId.On;
            }

            return BinaryStateId.Off;
        }

        ////private void Animate(AnimateParameter animateParameter, IComponentState newState)
        ////{
        ////    var directionAnimation = new DirectionAnimation(_timer);
        ////    directionAnimation.WithActuator(this);
        ////    directionAnimation.WithTargetState(newState);

        ////    if (animateParameter.Reverse)
        ////    {
        ////        directionAnimation.WithReversed();
        ////    }

        ////    directionAnimation.Start();
        ////}
    }
}