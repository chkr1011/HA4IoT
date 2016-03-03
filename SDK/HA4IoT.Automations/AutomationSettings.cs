using System;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Core.Settings;
using HA4IoT.Networking;

namespace HA4IoT.Automations
{
    public class AutomationSettings : SettingsContainer, IAutomationSettings
    {
        public AutomationSettings(AutomationId automationId, IHttpRequestController httpApiController, ILogger logger)
            : base(GenerateFilename(automationId), logger)
        {
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));

            AutomationId = automationId;
            IsEnabled = new Setting<bool>(true);
            AppSettings = new Setting<JsonObject>(new JsonObject());

            new AutomationSettingsHttpApiDispatcher(this, httpApiController).ExposeToApi();
        }

        [HideFromToJsonObject]
        public AutomationId AutomationId { get; }

        public Setting<bool> IsEnabled { get; }

        public Setting<JsonObject> AppSettings { get; }
         
        private static string GenerateFilename(AutomationId automationId)
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, "Automations", automationId.Value, "Settings.json");
        }
    }
}
