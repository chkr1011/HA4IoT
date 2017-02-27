using System;
using System.Collections.Generic;
using HA4IoT.Components;
using HA4IoT.Contracts.Commands;

namespace HA4IoT.Commands
{
    public class CommandExecutor
    {
        private readonly Dictionary<Type, IcommandExecutorAction> _actions = new Dictionary<Type, IcommandExecutorAction>();

        public void Register<TCommand>() where TCommand : ICommand
        {
            _actions.Add(typeof(TCommand), new commandExecutorAction<TCommand>(c => {}));
        }

        public void Register<TCommand>(Action<TCommand> callback) where TCommand : ICommand
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            _actions.Add(typeof(TCommand), new commandExecutorAction<TCommand>(callback));
        }

        public void Invoke(ICommand command)
        {
            IcommandExecutorAction action;
            if (!_actions.TryGetValue(command.GetType(), out action))
            {
                throw new CommandNotSupportedException(command);
            }

            action.Invoke(command);
        }
    }
}
