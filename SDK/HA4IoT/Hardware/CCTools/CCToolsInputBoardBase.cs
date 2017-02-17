using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.CCTools
{
    public abstract class CCToolsInputBoardBase : CCToolsBoardBase
    {
        protected CCToolsInputBoardBase(string id, IPortExpanderDriver portExpanderDriver) 
            : base(id, portExpanderDriver)
        {
        }

        public bool AutomaticallyFetchState { get; set; }
    }
}
