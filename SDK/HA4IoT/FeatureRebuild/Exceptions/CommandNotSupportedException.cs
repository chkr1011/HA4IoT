using System;
using HA4IoT.FeatureRebuild.Commands;

namespace HA4IoT.FeatureRebuild.Exceptions
{
    public class CommandNotSupportedException : Exception
    {
        public CommandNotSupportedException(ICommand command) : 
            base($"Command of type '{command.GetType().FullName}' is not supported.")
        {
            Command = command;
        }
       
        public ICommand Command { get; }
    }
}
