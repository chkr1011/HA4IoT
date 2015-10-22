using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Actuators.Contracts;
using HA4IoT.Core.Timer;
using HA4IoT.Networking;
using HA4IoT.Notifications;

namespace HA4IoT.Actuators
{
    public class CombinedBinaryStateActuators : ActuatorBase, IBinaryStateOutputActuator
    {
        private readonly IHomeAutomationTimer _timer;
        ////private bool _animationsEnabled;

        public CombinedBinaryStateActuators(string id, IHttpRequestController httpApiController, INotificationHandler notificationHandler, IHomeAutomationTimer timer) : base(
                id, httpApiController, notificationHandler)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            _timer = timer;
        }

        public event EventHandler<BinaryActuatorStateChangedEventArgs> StateChanged;

        public IList<IBinaryStateOutputActuator> Actuators { get; } = new List<IBinaryStateOutputActuator>();

        public BinaryActuatorState State => Actuators.First().State;

        public void SetState(BinaryActuatorState state, bool commit = true)
        {
            BinaryActuatorState oldState = State;

            ////if (_animationsEnabled)
            ////{
            ////    var directionAnimation = new DirectionAnimation(_timer).WithForwardDirection().WithActuator(this);
            ////    directionAnimation.Start();

            ////    StateChanged?.Invoke(this, new BinaryActuatorStateChangedEventArgs(oldState, state));
            ////    return;
            ////}

            // Set the state of the actuators without a commit to ensure that the state is applied at once without a delay.
            foreach (var actuator in Actuators)
            {
                actuator.SetState(state, false);
            }

            if (commit)
            {
                foreach (var actuator in Actuators)
                {
                    actuator.SetState(state);
                }
            }

            StateChanged?.Invoke(this, new BinaryActuatorStateChangedEventArgs(oldState, state));
        }

        public void Toggle(bool commit = true)
        {
            SetState(Actuators.First().State == BinaryActuatorState.On ? BinaryActuatorState.Off : BinaryActuatorState.On, commit);
        }

        public void TurnOff(bool commit = true)
        {
            SetState(BinaryActuatorState.Off, commit);
        }

        public void TurnOn(bool commit = true)
        {
            SetState(BinaryActuatorState.On, commit);
        }

        public CombinedBinaryStateActuators WithActuator(IBinaryStateOutputActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            Actuators.Add(actuator);
            return this;
        }

        public CombinedBinaryStateActuators WithMaster(IBinaryStateOutputActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            Actuators.Insert(0, actuator);
            return this;
        }

        ////public CombinedBinaryStateActuators WithEnabledAnimations()
        ////{
        ////    _animationsEnabled = true;
        ////    return this;
        ////}
    }
}