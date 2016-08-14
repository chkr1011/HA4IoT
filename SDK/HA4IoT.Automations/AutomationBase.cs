using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Networking;
using HA4IoT.Settings;

namespace HA4IoT.Automations
{
    public abstract class AutomationBase : IAutomation
    {
        protected AutomationBase(AutomationId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
            Settings = new SettingsContainer(StoragePath.WithFilename("Automations", id.Value, "Settings.json"));
            GeneralSettingsWrapper = new AutomationSettingsWrapper(Settings);
        }

        public AutomationId Id { get; }

        public ISettingsContainer Settings { get; }
        public IAutomationSettingsWrapper GeneralSettingsWrapper { get; }

        public virtual JsonObject ExportConfigurationAsJsonValue()
        {
            var result = new JsonObject();
            result.SetNamedString("type", GetType().Name);

            if (Settings != null)
            {
                result.SetNamedValue("settings", Settings.Export());
            }

            return result;
        }

        public virtual JsonObject ExportStatusToJsonObject()
        {
            return new JsonObject();
        }
    }
}
