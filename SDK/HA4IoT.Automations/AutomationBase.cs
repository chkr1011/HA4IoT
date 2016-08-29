using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Automations;
using HA4IoT.Networking.Json;

namespace HA4IoT.Automations
{
    public abstract class AutomationBase : IAutomation
    {
        protected AutomationBase(AutomationId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public AutomationId Id { get; }
        
        public virtual JsonObject ExportStatusToJsonObject()
        {
            return new JsonObject();
        }
    }
}
