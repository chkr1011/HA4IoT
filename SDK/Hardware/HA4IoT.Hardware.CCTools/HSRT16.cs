using HA4IoT.Contracts;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Hardware.GenericIOBoard;
using HA4IoT.Hardware.PortExpanderDrivers;
using HA4IoT.Notifications;

namespace HA4IoT.Hardware.CCTools
{
    public class HSRT16 : IOBoardController, IBinaryOutputController
    {
        public HSRT16(string id, int address, II2cBusAccessor i2CBus, INotificationHandler notificationHandler)
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