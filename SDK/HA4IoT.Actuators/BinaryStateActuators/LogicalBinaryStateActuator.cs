using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Actuators.Animations;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators.BinaryStateActuators
{
    public class LogicalBinaryStateActuator : StateMachine
    {
        private readonly IHomeAutomationTimer _timer;

        public LogicalBinaryStateActuator(ComponentId id, IHomeAutomationTimer timer) 
            : base(id)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;

            AddState(new StateMachineState(BinaryStateId.Off).WithAction(p => ApplyState(BinaryStateId.Off, p)));
            AddState(new StateMachineState(BinaryStateId.On).WithAction(p => ApplyState(BinaryStateId.On, p)));

            SetInitialState(BinaryStateId.Off);
        }

        public IList<IStateMachine> Actuators { get; } = new List<IStateMachine>();

        public LogicalBinaryStateActuator WithActuator(IStateMachine actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            Actuators.Add(actuator);
            return this;
        }

        private void ApplyState(StatefulComponentState stateId, params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            var animationParameter = parameters.SingleOrDefault(p => p is AnimateParameter) as AnimateParameter;
            if (animationParameter != null)
            {
                Animate(animationParameter, stateId);
                return;
            }

            // Set the state of the actuators without a commit to ensure that the state is applied at once without a delay.
            foreach (var actuator in Actuators)
            {
                actuator.SetState(stateId, HardwareParameter.IsPartOfPartialUpdate);
            }

            bool commit = !parameters.Any(p => p is IsPartOfPartialUpdateParameter);
            bool forceUpdate = parameters.Any(p => p is ForceUpdateStateParameter);
            if (!commit && !forceUpdate)
            {
                return;
            }

            foreach (var actuator in Actuators)
            {
                actuator.SetState(stateId);
            }
        }

        ////protected override BinaryActuatorState GetStateInternal()
        ////{
        ////    if (!Actuators.Any())
        ////    {
        ////        return BinaryActuatorState.Off;
        ////    }

        ////    if (Actuators.Any(a => a.GetState() == DefaultStateIDs.On))
        ////    {
        ////        return DefaultStateIDs.On;
        ////    }

        ////    return DefaultStateIDs.Off;
        ////}

        private void Animate(AnimateParameter animateParameter, StatefulComponentState newState)
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