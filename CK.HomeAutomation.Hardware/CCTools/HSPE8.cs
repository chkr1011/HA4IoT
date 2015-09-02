using CK.HomeAutomation.Core;
using CK.HomeAutomation.Hardware.Drivers;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Hardware.CCTools
{
    public class HSPE8 : IOBoardController, IOutputController
    {
        public HSPE8(string id, int address, I2CBus bus, INotificationHandler notificationHandler)
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
