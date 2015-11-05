using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Actuators.Animations;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Core.Timer;
using HA4IoT.Networking;
using HA4IoT.Notifications;

namespace HA4IoT.Actuators
{
    public class LogicalBinaryStateOutputActuator : BinaryStateOutputActuatorBase
    {
        private readonly IHomeAutomationTimer _timer;

        public LogicalBinaryStateOutputActuator(string id, IHttpRequestController httpApiController, INotificationHandler notificationHandler,
            IHomeAutomationTimer timer) : base(
                id, httpApiController, notificationHandler)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;
        }

        public IList<IBinaryStateOutputActuator> Actuators { get; } = new List<IBinaryStateOutputActuator>();

        protected override void SetStateInternal(BinaryActuatorState newState, params IParameter[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            BinaryActuatorState oldState = GetStateInternal();

            var animationParameter = parameters.SingleOrDefault(p => p is AnimateParameter) as AnimateParameter;
            if (animationParameter != null)
            {
                var directionAnimation = new DirectionAnimation(_timer);
                directionAnimation.WithActuator(this);
                directionAnimation.WithTargetState(newState);

                if (animationParameter.Reverse)
                {
                    directionAnimation.WithReversed();
                }

                directionAnimation.Start();
                
                OnStateChanged(oldState, newState);
                return;
            }

            // Set the state of the actuators without a commit to ensure that the state is applied at once without a delay.
            foreach (var actuator in Actuators)
            {
                actuator.SetState(newState, new DoNotCommitStateParameter());
            }

            bool commit = !parameters.Any(p => p is DoNotCommitStateParameter);
            if (commit)
            {
                foreach (var actuator in Actuators)
                {
                    actuator.SetState(newState);
                }
            }

            OnStateChanged(newState, GetStateInternal());
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
        
        public LogicalBinaryStateOutputActuator WithActuator(IBinaryStateOutputActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            Actuators.Add(actuator);
            return this;
        }
    }
}