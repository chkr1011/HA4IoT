using System;
using System.Collections.Generic;
using System.Linq;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public class CombinedBinaryStateActuators : BaseActuator, IBinaryStateOutputActuator
    {
        private readonly List<IBinaryStateOutputActuator> _actuators = new List<IBinaryStateOutputActuator>();

        public CombinedBinaryStateActuators(string id, HttpRequestController httpApiController, INotificationHandler notificationHandler) : base(
                id, httpApiController, notificationHandler)
        {
        }

        public event EventHandler StateChanged;

        public BinaryActuatorState State => _actuators.First().State;

        public void SetState(BinaryActuatorState state, bool commit = true)
        {
            // Set the state of the actuators without a commit to ensure that the state is applied at once without a delay.
            foreach (var actuator in _actuators)
            {
                actuator.SetState(state, false);
            }

            if (commit)
            {
                foreach (var actuator in _actuators)
                {
                    actuator.SetState(state);
                }
            }

            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Toggle(bool commit = true)
        {
            SetState(_actuators.First().State == BinaryActuatorState.On ? BinaryActuatorState.Off : BinaryActuatorState.On, commit);
        }

        public void TurnOff(bool commit = true)
        {
            SetState(BinaryActuatorState.Off, commit);
        }

        public CombinedBinaryStateActuators WithActuator(IBinaryStateOutputActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            _actuators.Add(actuator);
            return this;
        }

        public CombinedBinaryStateActuators WithMaster(IBinaryStateOutputActuator actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            _actuators.Insert(0, actuator);
            return this;
        }
    }
}