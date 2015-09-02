using CK.HomeAutomation.Core;
using CK.HomeAutomation.Hardware.Drivers;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Hardware.CCTools
{
    public class HSPE16InputOnly : IOBoardController, IInputController
    {
        public HSPE16InputOnly(string id, int address, I2CBus i2cBus, INotificationHandler notificationHandler)
            : base(id, new MAX7311Driver(address, i2cBus), notificationHandler)
        {
            byte[] setupAsInputs = {0x06, 0xFF, 0xFF};
            i2cBus.Execute(address, b => b.Write(setupAsInputs));

            FetchState();
        }

        public IBinaryInput GetInput(int number)
        {
            // All ports have a pullup resistor.
            return GetPort(number).WithInvertedState();
        }
    }
}
