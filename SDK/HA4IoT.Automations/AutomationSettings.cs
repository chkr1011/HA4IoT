using System;
using System.IO;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core;
using HA4IoT.Core.Settings;
using HA4IoT.Networking;

namespace HA4IoT.Automations
{
    public class AutomationSettings : SettingsContainer, IAutomationSettings
    {
        public AutomationSettings(AutomationId automationId, IApiController apiController)
            : base(GenerateFilename(automationId))
        {
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            AutomationId = automationId;
            IsEnabled = new Setting<bool>(true);
            AppSettings = new Setting<JsonObject>(new JsonObject());

            new AutomationSettingsApiDispatcher(this, apiController).ExposeToApi();
        }

        [HideFromToJsonObject]
        public AutomationId AutomationId { get; }

        public Setting<bool> IsEnabled { get; }

        public Setting<JsonObject> AppSettings { get; }
         
        private static string GenerateFilename(AutomationId automationId)
        {
            string oldFilename = StoragePath.WithFilename("Automations", automationId.Value, "Configuration.json");
            string newFilename = StoragePath.WithFilename("Automations", automationId.Value, "Settings.json");

            if (File.Exists(oldFilename))
            {
                if (File.Exists(newFilename))
                {
                    File.Delete(oldFilename);
                }

                File.Move(oldFilename, newFilename);
            }

            return newFilename;
        }
    }
}
