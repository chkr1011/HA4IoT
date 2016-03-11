using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Actuators
{
    public class VirtualButton : ButtonBase
    {
        public VirtualButton(ActuatorId id, IApiController apiController, ILogger logger)
            : base(id, apiController, logger)
        {
        }
    }
}
