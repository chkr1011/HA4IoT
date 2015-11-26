using HA4IoT.Contracts;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class Socket : BinaryStateOutputActuator
    {
        public Socket(ActuatorId id, IBinaryOutput output, IHttpRequestController api, INotificationHandler log)
            : base(id, output, api, log)
        {
        }
    }
}