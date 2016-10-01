using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Actuators.Triggers
{
    public abstract class TriggerBase : ITrigger
    {
        private readonly List<Action> _actions = new List<Action>();

        public event EventHandler<TriggeredEventArgs> Triggered;

        public bool IsAnyAttached => Triggered != null || _actions.Count > 0;

        public void Attach(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _actions.Add(action);
        }

        public void Attach(IAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _actions.Add(action.Execute);
        }

        public void Execute()
        {
            Triggered?.Invoke(this, TriggeredEventArgs.Empty);

            foreach (var action in _actions)
            {
                action();
            }
        }
    }
}
