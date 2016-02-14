using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Logging;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class VirtualButton : ButtonBase
    {
        public VirtualButton(ActuatorId id, IHttpRequestController api, ILogger logger)
            : base(id, api, logger)
        {
        }
    }
}
