using HA4IoT.Contracts.Commands;

namespace HA4IoT.Components
{
    public interface ICommandExecutorAction
    {
        void Execute(ICommand command);
    }
}