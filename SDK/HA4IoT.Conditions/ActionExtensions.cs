using System;
using HA4IoT.Contracts.Actions;

namespace HA4IoT.Conditions
{
    public static class ActionExtensions
    {
        public static ConditionalAction ToConditionalAction(this IAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            return new ConditionalAction().WithAction(action);
        }
    }
}
