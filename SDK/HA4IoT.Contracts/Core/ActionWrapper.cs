using System;

namespace HA4IoT.Contracts.Core
{
    public class ActionWrapper : IAction
    {
        private readonly Action _action;

        public ActionWrapper(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _action = action;
        }

        public void Execute()
        {
            _action();
        }
    }
}
