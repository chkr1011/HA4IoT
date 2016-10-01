using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Actuators.BinaryStateActuators
{
    public class LogicalBinaryStateActuator : ActuatorBase, IStateMachine
    {
        private readonly ITimerService _timerService;

        private ComponentState _state = new ComponentState(null);

        public LogicalBinaryStateActuator(ComponentId id, ITimerService timerService) 
            : base(id)
        {
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));

            _timerService = timerService;
        }

        public IList<IStateMachine> Actuators { get; } = new List<IStateMachine>();

        public LogicalBinaryStateActuator WithActuator(IStateMachine actuator)
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

                OnActiveStateChanged(oldState, _state);
            };

            Actuators.Add(actuator);
            _state = GetStateInternal();

            return this;
        }

        public override ComponentState GetState()
        {
            return _state;
        }

        public override void ResetState()
        {
            SetState(BinaryStateId.Off, new ForceUpdateStateParameter());
        }

        public override void SetState(ComponentState state, params IHardwareParameter[] parameters)
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

        public ComponentState GetNextState(ComponentState baseStateId)
        {
            if (baseStateId.Equals(BinaryStateId.Off))
            {
                return BinaryStateId.On;
            }

            if (baseStateId.Equals(BinaryStateId.On))
            {
                return BinaryStateId.Off;
            }
            
            throw new StateNotSupportedException(baseStateId);
        }

        public void SetStateIdAlias(ComponentState id, ComponentState alias)
        {
            throw new NotSupportedException();
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

        public override IList<ComponentState> GetSupportedStates()
        {
            return new List<ComponentState> {BinaryStateId.Off, BinaryStateId.On};
        }

        private ComponentState GetStateInternal()
        {
            if (Actuators.Any(a => a.GetState().Equals(BinaryStateId.On)))
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