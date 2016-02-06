using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Hardware.PortExpanderDrivers;
using HA4IoT.Networking;

namespace HA4IoT.Hardware.CCTools
{
    public class HSPE16OutputOnly : CCToolsBoardBase, IBinaryOutputController
    {
        public HSPE16OutputOnly(DeviceId id, I2CSlaveAddress address, II2CBus i2cBus, IHttpRequestController httpApi, INotificationHandler logger)
            : base(id, new MAX7311Driver(address, i2cBus), httpApi, logger)
        {
            CommitChanges(true);
        }

        public IBinaryOutput GetOutput(int number)
        {
            return GetPort(number);
        }

        public IBinaryOutput this[HSPE16Pin pin] => GetOutput((int)pin);
    }
}
