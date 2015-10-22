using CK.HomeAutomation.Hardware.GenericIOBoard;
using CK.HomeAutomation.Hardware.PortExpanderDrivers;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Hardware.CCTools
{
    public class HSPE16InputOnly : IOBoardController, IBinaryInputController
    {
        public HSPE16InputOnly(string id, int address, II2cBusAccessor i2cBus, INotificationHandler notificationHandler)
            : base(id, new MAX7311Driver(address, i2cBus), notificationHandler)
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
    }
}
