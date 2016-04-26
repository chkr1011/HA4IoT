using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.CCTools
{
    public class CCToolsInputBoardBase : CCToolsBoardBase
    {
        public CCToolsInputBoardBase(DeviceId id, IPortExpanderDriver portExpanderDriver) 
            : base(id, portExpanderDriver)
        {
        }

        public bool AutomaticallyFetchState { get; set; }
    }
}
