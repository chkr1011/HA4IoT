using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Logging;
using HA4IoT.Hardware.I2C.I2CPortExpanderDrivers;

namespace HA4IoT.Hardware.CCTools.Devices
{
    public class HSPE8InputOnly : CCToolsInputDeviceBase
    {
        public HSPE8InputOnly(string id, I2CSlaveAddress address, II2CBusService i2CBusService, ILogger log)
            : base(id, new PCF8574Driver(address, i2CBusService), log)
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
