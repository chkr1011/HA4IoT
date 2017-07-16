using HA4IoT.Contracts.Components.Commands;

namespace HA4IoT.Components
{
    public interface ICommandExecutorAction
    {
        void Execute(ICommand command);
    }
}