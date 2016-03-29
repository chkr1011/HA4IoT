using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Core.Settings;

namespace HA4IoT.Core
{
    public class AreaSettingsApiDispatcher : SettingsContainerApiDispatcher
    {
        public AreaSettingsApiDispatcher(IArea area, IApiController apiController)
            : base(area.Settings, $"area/{area.Id}", apiController)
        {
        }
    }
}
