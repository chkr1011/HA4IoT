using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Automations;
using HA4IoT.Networking;

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

        public JsonObject GetConfigurationForApi()
        {
            var result = new JsonObject();
            result.SetNamedValue("type", GetType().FullName.ToJsonValue());
            return result;
        }

        public JsonObject GetStatusForApi()
        {
            return new JsonObject();
        }
    }
}
