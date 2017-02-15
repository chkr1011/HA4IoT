using HA4IoT.Contracts.Commands;

namespace HA4IoT.Components
{
    public interface ICommandInvokerAction
    {
        void Invoke(ICommand command);
    }
}