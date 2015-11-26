using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Hardware.GenericIOBoard;
using HA4IoT.Hardware.PortExpanderDrivers;

namespace HA4IoT.Hardware.CCTools
{
    public class HSPE8 : IOBoardControllerBase, IBinaryOutputController
    {
        public HSPE8(string id, I2CSlaveAddress address, II2CBus bus, INotificationHandler notificationHandler)
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
