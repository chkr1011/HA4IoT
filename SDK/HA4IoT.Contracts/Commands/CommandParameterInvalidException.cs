using System;

namespace HA4IoT.Contracts.Commands
{
    public class CommandParameterInvalidException : Exception
    {
        public CommandParameterInvalidException(string message) : base(message)
        {   
        }
    }
}
