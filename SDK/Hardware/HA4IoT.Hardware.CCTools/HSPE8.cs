using CK.HomeAutomation.Hardware.GenericIOBoard;
using CK.HomeAutomation.Hardware.PortExpanderDrivers;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Hardware.CCTools
{
    public class HSPE8 : IOBoardController, IBinaryOutputController
    {
        public HSPE8(string id, int address, II2cBusAccessor bus, INotificationHandler notificationHandler)
            : base(id, new PCF8574Driver(address, bus), notificationHandler)
        {
            FetchState();
        }

        public IBinaryOutput GetOutput(int number)
        {
            return GetPort(number);
        }
    }
}
