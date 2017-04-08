using System;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Actuators
{
    public class StateChangedEventArgs : EventArgs
    {
        public StateChangedEventArgs(JToken oldState, JToken newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public JToken OldState { get; }

        public JToken NewState { get; }
    }
}
