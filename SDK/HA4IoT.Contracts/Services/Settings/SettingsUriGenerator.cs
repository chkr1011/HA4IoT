using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Contracts.Services.Settings
{
    public static class SettingsUriGenerator
    {
        public static string From<TService>() where TService : IService
        {
            return "Service/" + typeof(TService).Name;
        }

        public static string From(ComponentId componentId)
        {
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));

            return "Component/" + componentId;
        }

        public static string From(AreaId areaId)
        {
            if (areaId == null) throw new ArgumentNullException(nameof(areaId));

            return "Area/" + areaId;
        }

        public static string From(AutomationId automationId)
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
