using System;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Settings
{
    public static class SettingsUriGenerator
    {
        public static string FromService<TService>() where TService : IService
        {
            return "Service/" + typeof(TService).Name;
        }

        public static string FromComponent(string componentId)
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));

            return "Component/" + componentId;
        }

        public static string FromArea(string areaId)
        {
            if (areaId == null) throw new ArgumentNullException(nameof(areaId));

            return "Area/" + areaId;
        }

        public static string FromAutomation(string automationId)
        {
            if (automationId == null) throw new ArgumentNullException(nameof(automationId));

            return "Automation/" + automationId;
        }

        public static string From(Type settingsType)
        {
            return settingsType.Name;
        }
    }
}
