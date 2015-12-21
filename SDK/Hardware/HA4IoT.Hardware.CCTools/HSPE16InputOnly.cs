using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Hardware.GenericIOBoard;
using HA4IoT.Hardware.PortExpanderDrivers;

namespace HA4IoT.Hardware.CCTools
{
    public class HSPE16InputOnly : IOBoardControllerBase, IBinaryInputController
    {
        public HSPE16InputOnly(string id, I2CSlaveAddress address, II2CBus i2cBus, INotificationHandler log)
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
