using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.CCTools
{
    public abstract class CCToolsInputBoardBase : CCToolsBoardBase
    {
        protected CCToolsInputBoardBase(string id, I2CIPortExpanderDriver portExpanderDriver) 
            : base(id, portExpanderDriver)
        {
        }

        public bool AutomaticallyFetchState { get; set; }
    }
}
