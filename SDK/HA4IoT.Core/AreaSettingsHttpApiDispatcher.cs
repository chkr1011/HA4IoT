using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Networking;
using HA4IoT.Core.Settings;

namespace HA4IoT.Core
{
    public class AreaSettingsHttpApiDispatcher : SettingsContainerHttpApiDispatcher<IAreaSettings>
    {
        public AreaSettingsHttpApiDispatcher(IAreaSettings settings, IHttpRequestController httpApiController)
            : base(settings, $"area/{settings.AreaId}", httpApiController)
        {
        }
    }
}
