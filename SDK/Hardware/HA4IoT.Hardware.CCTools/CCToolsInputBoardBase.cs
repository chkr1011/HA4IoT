using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsInputBoardBase : CCToolsBoardBase
    {
        public CCToolsInputBoardBase(DeviceId id, IPortExpanderDriver portExpanderDriver, IApiController apiController) 
            : base(id, portExpanderDriver, apiController)
        {
        }

        public bool AutomaticallyFetchState { get; set; }
    }
}
