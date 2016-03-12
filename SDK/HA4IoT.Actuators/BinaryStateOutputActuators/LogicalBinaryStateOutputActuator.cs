using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Actuators.Animations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Actuators
{
    public class LogicalBinaryStateOutputActuator : BinaryStateOutputActuatorBase<ActuatorSettings>
    {
        private readonly IHomeAutomationTimer _timer;

        public LogicalBinaryStateOutputActuator(ActuatorId id, IApiController apiController, ILogger logger, IHomeAutomationTimer timer) 
            : base(id, apiController, logger)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;

            Settings = new ActuatorSettings(id, logger);
        }

        public IList<IBinaryStateOutputActuator> Actuators { get; } = new List<IBinaryStateOutputActuator>();

        public LogicalBinaryStateOutputActuator WithActuator(IBinaryStateOutputActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            Actuators.Add(actuator);
            return this;
        }

        protected override void SetStateInternal(BinaryActuatorState state, params IHardwareParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            BinaryActuatorState oldState = GetStateInternal();

            var animationParameter = parameters.SingleOrDefault(p => p is AnimateParameter) as AnimateParameter;
            if (animationParameter != null)
            {
                Animate(animationParameter, oldState, state);
                return;
            }

            // Set the state of the actuators without a commit to ensure that the state is applied at once without a delay.
            foreach (var actuator in Actuators)
            {
                actuator.SetState(state, new IsPartOfPartialUpdateParameter());
            }

            bool commit = !parameters.Any(p => p is IsPartOfPartialUpdateParameter);
            bool forceUpdate = parameters.Any(p => p is ForceUpdateStateParameter);
            if (!commit && !forceUpdate)
            {
                return;
            }

            foreach (var actuator in Actuators)
            {
                actuator.SetState(state);
            }

            OnStateChanged(oldState, GetStateInternal());
        }

        protected override BinaryActuatorState GetStateInternal()
        {
            if (!Actuators.Any())
            {
                return BinaryActuatorState.Off;
            }

            if (Actuators.Any(a => a.GetState() == BinaryActuatorState.On))
            {
                return BinaryActuatorState.On;
            }

            return BinaryActuatorState.Off;
        }

        private void Animate(AnimateParameter animateParameter, BinaryActuatorState oldState, BinaryActuatorState newState)
        {
            var directionAnimation = new DirectionAnimation(_timer);
            directionAnimation.WithActuator(this);
            directionAnimation.WithTargetState(newState);

            if (animateParameter.Reverse)
            {
                directionAnimation.WithReversed();
            }

            directionAnimation.Start();

            OnStateChanged(oldState, newState);
        }
    }
}