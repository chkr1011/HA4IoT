using CK.HomeAutomation.Hardware.GenericIOBoard;
using CK.HomeAutomation.Hardware.PortExpanderDrivers;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Hardware.CCTools
{
    public class HSREL8 : IOBoardController, IBinaryOutputController
    {
        public HSREL8(string id, int address, II2cBusAccessor i2cBus, INotificationHandler notificationHandler)
            : base(id, new MAX7311Driver(address, i2cBus), notificationHandler)
        {
            SetState(new byte[] { 0x00, 255 });
            CommitChanges(true);
        }

        public IBinaryOutput GetOutput(int number)
        {
            return GetPort(number);
        }
    }
}