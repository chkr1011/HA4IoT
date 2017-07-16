using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Components.Commands;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Components.Commands
{
    public class CommandResolver
    {
        private readonly HashSet<Type> _commands = new HashSet<Type>();

        public CommandResolver()
        {
            RegisterCommands();
        }

        public ICommand Resolve(string type, JObject source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var commandType = _commands.FirstOrDefault(t => t.Name.Equals(type));
            if (commandType == null)
            {
                throw new CommandUnknownException(type);
            }

            return (ICommand)source.ToObject(commandType);
        }

        public void RegisterCommand<TCommand>()
        {
            _commands.Add(typeof(TCommand));
        }

        private void RegisterCommands()
        {
            RegisterCommand<TurnOffCommand>();
            RegisterCommand<TurnOnCommand>();
            RegisterCommand<TogglePowerStateCommand>();

            RegisterCommand<SetLevelCommand>();
            RegisterCommand<IncreaseLevelCommand>();
            RegisterCommand<DecreaseLevelCommand>();

            RegisterCommand<MoveUpCommand>();
            RegisterCommand<MoveDownCommand>();

            RegisterCommand<ResetCommand>();

            RegisterCommand<PressCommand>();

            RegisterCommand<SetStateCommand>();
            RegisterCommand<SetColorCommand>();

            RegisterCommand<TriggerMotionDetectionCommand>();
        }       
    }
}
