using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Hardware.GenericIOBoard;
using HA4IoT.Hardware.PortExpanderDrivers;

namespace HA4IoT.Hardware.CCTools
{
    public class HSRT16 : IOBoardControllerBase, IBinaryOutputController
    {
        public HSRT16(string id, I2CSlaveAddress address, II2CBus i2CBus, INotificationHandler notificationHandler)
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