using System;
using HA4IoT.Contracts.Automations;

namespace HA4IoT.Automations
{
    public abstract class AutomationBase : IAutomation
    {
        protected AutomationBase(string id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        public string Id { get; }
    }
}
