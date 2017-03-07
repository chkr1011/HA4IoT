using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.I2C.I2CPortExpanderDrivers;

namespace HA4IoT.Hardware.CCTools
{
    public class HSPE16InputOnly : CCToolsInputBoardBase, IBinaryInputController
    {
        public HSPE16InputOnly(string id, I2CSlaveAddress address, II2CBusService i2cBus, ILogger log)
            : base(id, new MAX7311Driver(address, i2cBus), log)
        {
            byte[] setupAsInputs = { 0x06, 0xFF, 0xFF };
            i2cBus.Execute(address, b => b.Write(setupAsInputs));

            FetchState();
        }

        public IBinaryInput GetInput(int number)
        {
            // All ports have a pullup resistor.
            return GetPort(number).WithInvertedState();
        }

        public IBinaryInput this[HSPE16Pin pin] => GetInput((int)pin);
    }
}
