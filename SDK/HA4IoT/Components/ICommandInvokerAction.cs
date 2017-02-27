using HA4IoT.Contracts.Commands;

namespace HA4IoT.Components
{
    public interface IcommandExecutorAction
    {
        void Invoke(ICommand command);
    }
}