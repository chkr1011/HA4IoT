using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Services.Settings
{
    public static class SettingsServiceExtensions
    {
        public static JsonObject GetRawSettings(this ISettingsService settingsService, AreaId areaId)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (areaId == null) throw new ArgumentNullException(nameof(areaId));

            var uri = SettingsUriGenerator.From(areaId);
            return settingsService.GetSettings(uri);
        }

        public static JsonObject GetRawSettings(this ISettingsService settingsService, ComponentId componentId)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));

            var uri = SettingsUriGenerator.From(componentId);
            return settingsService.GetSettings(uri);
        }

        public static JsonObject GetRawSettings(this ISettingsService settingsService, AutomationId automationId)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (automationId == null) throw new ArgumentNullException(nameof(automationId));

            var uri = SettingsUriGenerator.From(automationId);
            return settingsService.GetSettings(uri);
        }

        public static TSettings GetSettings<TSettings>(this ISettingsService settingsService)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            // TODO: Search for attribute.

            var uri = SettingsUriGenerator.From(typeof(TSettings));
            return settingsService.GetSettings<TSettings>(uri);
        }

        public static TSettings GetSettings<TSettings>(this ISettingsService settingsService, ComponentId componentId) where TSettings : IComponentSettings
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            // TODO: Search for attribute.

            var uri = SettingsUriGenerator.From(componentId);
            return settingsService.GetSettings<TSettings>(uri);
        }

        public static TSettings GetSettings<TSettings>(this ISettingsService settingsService, AutomationId automationId) where TSettings : IAutomationSettings
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            // TODO: Search for attribute.

            var uri = SettingsUriGenerator.From(automationId);
            return settingsService.GetSettings<TSettings>(uri);
        }
    }
}
