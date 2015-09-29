using CK.HomeAutomation.Hardware;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public class Lamp : BinaryStateOutput
    {
        public Lamp(string id, IBinaryOutput output, IHttpRequestController httpRequestController, INotificationHandler notificationHandler)
            : base(id, output, httpRequestController, notificationHandler)
        {
        }
    }
}