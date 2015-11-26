using HA4IoT.Contracts;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Actuators
{
    public class Lamp : BinaryStateOutputActuator
    {
        public Lamp(ActuatorId id, IBinaryOutput output, IHttpRequestController request, INotificationHandler log)
            : base(id, output, request, log)
        {
        }
    }
}