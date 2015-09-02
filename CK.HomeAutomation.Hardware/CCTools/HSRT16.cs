using CK.HomeAutomation.Core;
using CK.HomeAutomation.Hardware.Drivers;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Hardware.CCTools
{
    public class HSRT16 : IOBoardController, IOutputController
    {
        public HSRT16(string id, int address, I2CBus i2CBus, INotificationHandler notificationHandler)
            : base(id, new MAX7311Driver(address, i2CBus), notificationHandler)
        {
            SetState(new byte[] { 0x00, 0x00 });
            CommitChanges(true);
        }

        public IBinaryOutput GetOutput(int number)
        {
            return GetPort(number);
        }
    }
}