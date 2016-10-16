using System;
using HA4IoT.Contracts.Actions;

namespace HA4IoT.Actuators.Actions
{
    public class Action : IAction
    {
        private readonly System.Action _callback;

        public Action(System.Action callback)
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
