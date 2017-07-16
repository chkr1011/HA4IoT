using System;

namespace HA4IoT.Contracts.Components.Commands
{
    public class CommandNotSupportedException : Exception
    {
        public CommandNotSupportedException(ICommand command)
            : base($"Command '{command.GetType().Name}' not supported.")
        {
            Command = command;
        }

        public ICommand Command { get; }
    }
}
