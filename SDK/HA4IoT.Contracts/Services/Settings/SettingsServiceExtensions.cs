using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Services.Settings
{
    public static class SettingsServiceExtensions
    {
        public static JObject GetRawAreaSettings(this ISettingsService settingsService, string areaId)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (areaId == null) throw new ArgumentNullException(nameof(areaId));

            var uri = SettingsUriGenerator.FromArea(areaId);
            return settingsService.GetSettings(uri);
        }

        public static JObject GetRawComponentSettings(this ISettingsService settingsService, string componentId)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));

            var uri = SettingsUriGenerator.FromComponent(componentId);
            return settingsService.GetSettings(uri);
        }

        public static JObject GetRawAutomationSettings(this ISettingsService settingsService, string automationId)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (automationId == null) throw new ArgumentNullException(nameof(automationId));

            var uri = SettingsUriGenerator.FromAutomation(automationId);
            return settingsService.GetSettings(uri);
        }

        public static TSettings GetSettings<TSettings>(this ISettingsService settingsService)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            
            var uri = SettingsUriGenerator.From(typeof(TSettings));
            return settingsService.GetSettings<TSettings>(uri);
        }

        public static TSettings GetComponentSettings<TSettings>(this ISettingsService settingsService, string componentId) where TSettings : ComponentSettings
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            var uri = SettingsUriGenerator.FromComponent(componentId);
            return settingsService.GetSettings<TSettings>(uri);
        }

        public static ComponentSettings GetComponentSettings(this ISettingsService settingsService, string componentId)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            var uri = SettingsUriGenerator.FromComponent(componentId);
            return settingsService.GetSettings<ComponentSettings>(uri);
        }

        public static TSettings GetAutomationSettings<TSettings>(this ISettingsService settingsService, string automationId) where TSettings : IAutomationSettings
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (automationId == null) throw new ArgumentNullException(nameof(automationId));

            var uri = SettingsUriGenerator.FromAutomation(automationId);
            return settingsService.GetSettings<TSettings>(uri);
        }

        public static void SetSettings<TSettings>(this ISettingsService settingsService, string automationId, TSettings settings) where TSettings : IAutomationSettings
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (automationId == null) throw new ArgumentNullException(nameof(automationId));

            var uri = SettingsUriGenerator.FromAutomation(automationId);

            settingsService.ImportSettings(uri, settings);
        }

        public static void CreateSettingsMonitor<TSettings>(this ISettingsService settingsService, Action<TSettings> callback)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            
            var uri = SettingsUriGenerator.From(typeof(TSettings));
            settingsService.CreateSettingsMonitor(uri, callback);
        }

        public static void CreateAreaSettingsMonitor(this ISettingsService settingsService, string areaId, Action<AreaSettings> callback)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            var uri = SettingsUriGenerator.FromArea(areaId);
            settingsService.CreateSettingsMonitor(uri, callback);
        }

        public static void CreateComponentSettingsMonitor<TSettings>(this ISettingsService settingsService, string componentId, Action<TSettings> callback) where TSettings : ComponentSettings
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            
            var uri = SettingsUriGenerator.FromComponent(componentId);
            settingsService.CreateSettingsMonitor(uri, callback);
        }

        public static void CreateAutomationSettingsMonitor<TSettings>(this ISettingsService settingsService, string automationId, Action<TSettings> callback) where TSettings : IAutomationSettings
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            
            var uri = SettingsUriGenerator.FromAutomation(automationId);
            settingsService.CreateSettingsMonitor(uri, callback);
        }
    }
}
