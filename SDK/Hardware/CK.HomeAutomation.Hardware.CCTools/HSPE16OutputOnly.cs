using CK.HomeAutomation.Hardware.GenericIOBoard;
using CK.HomeAutomation.Hardware.PortExpanderDrivers;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Hardware.CCTools
{
    public class HSPE16OutputOnly : IOBoardController, IOutputController
    {
        public HSPE16OutputOnly(string id, int address, II2cBusAccessor i2cBus, INotificationHandler notificationHandler)
            : base(id, new MAX7311Driver(address, i2cBus), notificationHandler)
        {
            CommitChanges(true);
        }

        public IBinaryOutput GetOutput(int number)
        {
            return GetPort(number);
        }
    }
}
