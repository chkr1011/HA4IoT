using System;

namespace HA4IoT.Contracts.Commands
{
    public class CommandParameterInvalidException : Exception
    {
        public CommandParameterInvalidException(ICommand command, string message) : base(message)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            Command = command;
        }

        public ICommand Command { get; }
    }
}
