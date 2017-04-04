using System;

namespace HA4IoT.Contracts.Commands
{
    public class CommandUnknownException : Exception
    {
        public CommandUnknownException(string commandType)
            : base($"Command '{commandType}' is unknown.")
        {
            CommandType = commandType;
        }

        public string CommandType { get; }
    }
}
