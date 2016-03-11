using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Core.Settings;

namespace HA4IoT.Core
{
    public class AreaSettingsHttpApiDispatcher : SettingsContainerHttpApiDispatcher<IAreaSettings>
    {
        public AreaSettingsHttpApiDispatcher(IAreaSettings settings, IApiController apiController)
            : base(settings, $"area/{settings.AreaId}", apiController)
        {
        }
    }
}
