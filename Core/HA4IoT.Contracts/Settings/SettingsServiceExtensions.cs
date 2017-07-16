using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using Newtonsoft.Json.Linq;

namespace HA4IoT.Contracts.Settings
{
    public static class SettingsServiceExtensions
    {
        public static JObject GetRawSettings(this ISettingsService settingsService, IArea area)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (area == null) throw new ArgumentNullException(nameof(area));

            var uri = SettingsUriGenerator.FromArea(area.Id);
            return settingsService.GetSettings(uri);
        }

        public static JObject GetRawSettings(this ISettingsService settingsService, IComponent component)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (component == null) throw new ArgumentNullException(nameof(component));

            var uri = SettingsUriGenerator.FromComponent(component.Id);
            return settingsService.GetSettings(uri);
        }

        public static JObject GetRawSettings(this ISettingsService settingsService, IAutomation automation)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            var uri = SettingsUriGenerator.FromAutomation(automation.Id);
            return settingsService.GetSettings(uri);
        }

        public static TSettings GetSettings<TSettings>(this ISettingsService settingsService)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            
            var uri = SettingsUriGenerator.From(typeof(TSettings));
            return settingsService.GetSettings<TSettings>(uri);
        }

        public static TSettings GetSettings<TSettings>(this ISettingsService settingsService, IAutomation automation) where TSettings : AutomationSettings
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            var uri = SettingsUriGenerator.FromAutomation(automation.Id);
            return settingsService.GetSettings<TSettings>(uri);
        }

        public static TSettings GetSettings<TSettings>(this ISettingsService settingsService, IComponent component) where TSettings : ComponentSettings
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            return settingsService.GetComponentSettings<TSettings>(component.Id);
        }

        public static TSettings GetComponentSettings<TSettings>(this ISettingsService settingsService, string componentId) where TSettings : ComponentSettings
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            var uri = SettingsUriGenerator.FromComponent(componentId);
            return settingsService.GetSettings<TSettings>(uri);
        }

        public static void SetSettings<TSettings>(this ISettingsService settingsService, IComponent component, TSettings settings) where TSettings : ComponentSettings
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (component == null) throw new ArgumentNullException(nameof(component));

            var uri = SettingsUriGenerator.FromComponent(component.Id);
            settingsService.ImportSettings(uri, settings);
        }

        public static void SetSettings<TSettings>(this ISettingsService settingsService, IAutomation automation, TSettings settings) where TSettings : AutomationSettings
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (automation == null) throw new ArgumentNullException(nameof(automation));

            var uri = SettingsUriGenerator.FromAutomation(automation.Id);
            settingsService.ImportSettings(uri, settings);
        }

        public static void SetComponentEnabledState(this ISettingsService settingsService, IComponent component, bool isEnabled)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (component == null) throw new ArgumentNullException(nameof(component));

            var settings = settingsService.GetComponentSettings<ComponentSettings>(component.Id);
            settings.IsEnabled = isEnabled;
            settingsService.SetSettings(component, settings);
        }

        public static void CreateSettingsMonitor<TSettings>(this ISettingsService settingsService, Action<SettingsChangedEventArgs<TSettings>> callback)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            
            var uri = SettingsUriGenerator.From(typeof(TSettings));
            settingsService.CreateSettingsMonitor(uri, callback);
        }

        public static void CreateSettingsMonitor(this ISettingsService settingsService, IArea area, Action<SettingsChangedEventArgs<AreaSettings>> callback)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            var uri = SettingsUriGenerator.FromArea(area.Id);
            settingsService.CreateSettingsMonitor(uri, callback);
        }

        public static void CreateSettingsMonitor<TSettings>(this ISettingsService settingsService, IComponent component, Action<SettingsChangedEventArgs<TSettings>> callback) where TSettings : ComponentSettings
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            
            var uri = SettingsUriGenerator.FromComponent(component.Id);
            settingsService.CreateSettingsMonitor(uri, callback);
        }

        public static void CreateSettingsMonitor<TSettings>(this ISettingsService settingsService, IAutomation automation, Action<SettingsChangedEventArgs<TSettings>> callback) where TSettings : AutomationSettings
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            
            var uri = SettingsUriGenerator.FromAutomation(automation.Id);
            settingsService.CreateSettingsMonitor(uri, callback);
        }
    }
}
