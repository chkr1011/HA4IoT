using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware.GenericIOBoard;
using HA4IoT.Hardware.PortExpanderDrivers;
using HA4IoT.Notifications;

namespace HA4IoT.Hardware.CCTools
{
    public class HSPE16OutputOnly : IOBoardController, IBinaryOutputController
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
