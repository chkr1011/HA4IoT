using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Contracts.Automations
{
    public interface IAutomationSettings : ISettingsContainer
    {
        AutomationId AutomationId { get; }
    }
}
