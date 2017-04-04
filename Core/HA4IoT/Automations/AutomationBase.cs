using System;
using HA4IoT.Contracts.Automations;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Automations
{
    public abstract class AutomationBase : IAutomation
    {
        protected AutomationBase(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public string Id { get; }
        
        public virtual JObject ExportStatusToJsonObject()
        {
            return new JObject();
        }
    }
}
