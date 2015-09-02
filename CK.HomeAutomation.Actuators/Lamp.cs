using CK.HomeAutomation.Core;
using CK.HomeAutomation.Networking;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Actuators
{
    public class Lamp : BinaryStateOutput
    {
        public Lamp(string id, IBinaryOutput output, HttpRequestController httpRequestController, INotificationHandler notificationHandler)
            : base(id, output, httpRequestController, notificationHandler)
        {
        }
    }
}