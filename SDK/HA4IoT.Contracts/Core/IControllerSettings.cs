using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Contracts.Core
{
    public interface IControllerSettings : ISettingsContainer
    {
        ISetting<string> Name { get; }
        ISetting<string> Description { get; }
    }
}