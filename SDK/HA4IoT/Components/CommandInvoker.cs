using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Commands;

namespace HA4IoT.Components
{
    public class CommandInvoker
    {
        private readonly Dictionary<Type, ICommandInvokerAction> _actions = new Dictionary<Type, ICommandInvokerAction>();

        public void Register<TCommand>(Action<TCommand> callback) where TCommand : ICommand
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            _actions.Add(typeof(TCommand), new CommandInvokerAction<TCommand>(callback));
        }

        public void Invoke(ICommand command)
        {
            ICommandInvokerAction action;
            if (!_actions.TryGetValue(command.GetType(), out action))
            {
                throw new CommandNotSupportedException(command);
            }

            action.Invoke(command);
        }
    }
}
