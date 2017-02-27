using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators.StateMachines
{
    public class PendingComponentCommand
    {
        public IComponent Component { get; set; }

        public ICommand Command { get; set; }

        public void Invoke()
        {
            Component?.ExecuteCommand(Command);
        }
    }
}
