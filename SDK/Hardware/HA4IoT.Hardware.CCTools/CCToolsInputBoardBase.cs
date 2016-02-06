using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Networking;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsInputBoardBase : CCToolsBoardBase
    {
        public CCToolsInputBoardBase(DeviceId id, IPortExpanderDriver portExpanderDriver, IHttpRequestController httpApi, INotificationHandler logger) 
            : base(id, portExpanderDriver, httpApi, logger)
        {
        }

        public bool AutomaticallyFetchState { get; set; }
    }
}
