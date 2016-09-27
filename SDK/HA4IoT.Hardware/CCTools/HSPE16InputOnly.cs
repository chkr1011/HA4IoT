using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.PortExpanderDrivers;

namespace HA4IoT.Hardware.CCTools
{
    public class HSPE16InputOnly : CCToolsInputBoardBase, IBinaryInputController
    {
        public HSPE16InputOnly(DeviceId id, I2CSlaveAddress address, II2CBusService i2cBus)
            : base(id, new MAX7311Driver(address, i2cBus))
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
