using System;
using HA4IoT.Contracts.Actions;

namespace HA4IoT.Actuators.Actions
{
    public class HomeAutomationAction : IHomeAutomationAction
    {
        private readonly Action _callback;

        public HomeAutomationAction(Action callback)
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
