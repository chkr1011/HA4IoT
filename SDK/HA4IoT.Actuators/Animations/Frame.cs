using System;
using System.Collections.Generic;

namespace HA4IoT.Actuators.Animations
{
    public class Frame
    {
        private readonly List<Action> _actions = new List<Action>();

        public TimeSpan StartTime { get; private set; }

        public Frame WithStartTime(TimeSpan startTime)
        {
            StartTime = startTime;
            return this;
        }

        public Frame WithAction(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _actions.Add(action);
            return this;
        }

        public void Apply()
        {
            foreach (var action in _actions)
            {
                action.Invoke();
            }
        }
    }
}
