using System;
using HA4IoT.Contracts.Actions;

namespace HA4IoT.Actuators.Actions
{
    public class ActuatorAction : IActuatorAction
    {
        private readonly Action _callback;

        public ActuatorAction(Action callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            _callback = callback;
        }

        public void Execute()
        {
            _callback();
        }
    }
}
