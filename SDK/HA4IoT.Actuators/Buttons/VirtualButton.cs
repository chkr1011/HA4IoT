using HA4IoT.Contracts;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;
using HA4IoT.Notifications;

namespace HA4IoT.Actuators
{
    public class VirtualButton : ButtonBase
    {
        public VirtualButton(string id, IHttpRequestController api, INotificationHandler log)
            : base(id, api, log)
        {
        }
    }
}
