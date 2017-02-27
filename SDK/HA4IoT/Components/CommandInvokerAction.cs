using System;
using HA4IoT.Contracts.Commands;

namespace HA4IoT.Components
{
    public class commandExecutorAction<TCommand> : IcommandExecutorAction where TCommand : ICommand
    {
        private readonly Action<TCommand> _command;

        public commandExecutorAction(Action<TCommand> command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _command = command;
        }

        public void Invoke(ICommand command)
        {
            _command((TCommand)command);
        }
    }
}