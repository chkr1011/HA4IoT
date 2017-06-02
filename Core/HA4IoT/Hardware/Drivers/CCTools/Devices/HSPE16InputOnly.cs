using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Logging;
using HA4IoT.Hardware.Drivers.I2CPortExpanderDrivers;

namespace HA4IoT.Hardware.Drivers.CCTools.Devices
{
    public sealed class HSPE16InputOnly : CCToolsDeviceBase
    {
        public HSPE16InputOnly(string id, I2CSlaveAddress address, II2CBusService i2CBusService, ILogger log)
            : base(id, new MAX7311Driver(address, i2CBusService), log)
        {
            byte[] setupAsInputs = { 0x06, 0xFF, 0xFF };
            i2CBusService.Write(address, setupAsInputs);

            FetchState();
        }

        public IBinaryInput GetInput(int number)
        {
            // All ports have a pullup resistor.
            return ((IBinaryInput)GetPort(number)).WithInvertedState();
        }

        public IBinaryInput this[HSPE16Pin pin] => GetInput((int)pin);
    }
}
