using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsInputBoardBase : CCToolsBoardBase
    {
        public CCToolsInputBoardBase(DeviceId id, IPortExpanderDriver portExpanderDriver, IApiController apiController, ILogger logger) 
            : base(id, portExpanderDriver, apiController, logger)
        {
        }

        public bool AutomaticallyFetchState { get; set; }
    }
}
