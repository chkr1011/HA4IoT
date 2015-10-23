using HA4IoT.Contracts.Hardware;
using HA4IoT.Networking;
using HA4IoT.Notifications;

namespace HA4IoT.Actuators
{
    public class Lamp : BinaryStateOutput
    {
        public Lamp(string id, IBinaryOutput output, IHttpRequestController httpRequestController, INotificationHandler notificationHandler)
            : base(id, output, httpRequestController, notificationHandler)
        {
        }
    }
}