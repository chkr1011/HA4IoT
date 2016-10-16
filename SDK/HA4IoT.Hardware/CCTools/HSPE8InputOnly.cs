using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.PortExpanderDrivers;

namespace HA4IoT.Hardware.CCTools
{
    public class HSPE8InputOnly : CCToolsInputBoardBase, IBinaryInputController
    {
        public HSPE8InputOnly(DeviceId id, I2CSlaveAddress address, II2CBusService bus)
            : base(id, new PCF8574Driver(address, bus))
        {
            FetchState();
        }

        public IBinaryInput GetInput(int number)
        {
            if (number < 0 || number > 7) throw new ArgumentOutOfRangeException(nameof(number));

            return GetPort(number);
        }

        public IBinaryInput this[HSPE8Pin pin] => GetInput((int)pin);
    }
}
