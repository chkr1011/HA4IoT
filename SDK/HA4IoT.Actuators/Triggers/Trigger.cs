using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Actuators.Triggers
{
    public class Trigger : ITrigger
    {
        private readonly List<Action> _actions = new List<Action>();

        public event EventHandler<TriggeredEventArgs> Triggered;

        public bool IsAnyAttached => Triggered != null || _actions.Count > 0;

        public void Attach(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _actions.Add(action);
        }

        public void Invoke()
        {
            Triggered?.Invoke(this, TriggeredEventArgs.Empty);

            foreach (var action in _actions)
            {
                action();
            }
        }
    }
}
