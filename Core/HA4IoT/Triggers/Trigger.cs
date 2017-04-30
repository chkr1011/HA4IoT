using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Triggers
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

        public void Attach(IAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _actions.Add(action.Execute);
        }

        public Task ExecuteAsync()
        {
            return Task.Run(() =>
            {
                Execute();
            });
        }

        public void Execute()
        {
            Triggered?.Invoke(this, new TriggeredEventArgs());

            foreach (var action in _actions)
            {
                action();
            }
        }
    }
}
