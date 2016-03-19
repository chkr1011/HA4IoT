using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Core.Settings;

namespace HA4IoT.Core
{
    public class AreaSettingsApiDispatcher : SettingsContainerApiDispatcher<IAreaSettings>
    {
        public AreaSettingsApiDispatcher(IAreaSettings settings, IApiController apiController)
            : base(settings, $"area/{settings.AreaId}", apiController)
        {
        }
    }
}
