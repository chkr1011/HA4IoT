using HA4IoT.Contracts.Core.Settings;

namespace HA4IoT.Contracts.Core
{
    public interface IController
    {
        ISettingsContainer Settings { get; }         
    }
}
