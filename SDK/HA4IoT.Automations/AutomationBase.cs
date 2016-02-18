using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Automations;
using HA4IoT.Networking;

namespace HA4IoT.Automations
{
    public abstract class AutomationBase<TSettings> : IAutomation where TSettings : AutomationSettings
    {
        protected AutomationBase(AutomationId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public AutomationId Id { get; }

        public TSettings Settings { get; protected set; }

        public virtual JsonObject ExportConfigurationAsJsonValue()
        {
            var result = new JsonObject();
            result.SetNamedValue("Type", GetType().FullName.ToJsonValue());

            if (Settings != null)
            {
                result.SetNamedValue("Settings", Settings.ExportToJsonObject());
            }

            return result;
        }

        public virtual JsonObject ExportStatusToJsonObject()
        {
            return new JsonObject();
        }

        public virtual void LoadSettings()
        {
            Settings?.Load();
        }
    }
}
