using System;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Hardware.GenericIOBoard;
using HA4IoT.Hardware.PortExpanderDrivers;
using HA4IoT.Notifications;

namespace HA4IoT.Hardware.CCTools
{
    public class HSREL8 : IOBoardController, IBinaryOutputController
    {
        public HSREL8(string id, int i2CAddress, II2cBusAccessor i2CBus, INotificationHandler notificationHandler)
            : base(id, new MAX7311Driver(i2CAddress, i2CBus), notificationHandler)
        {
            SetState(new byte[] { 0x00, 255 });
            CommitChanges(true);
        }

        public IBinaryOutput GetOutput(int number)
        {
            if (number < 0 || number > 15) throw new ArgumentOutOfRangeException(nameof(number));

            return GetPort(number);
        }

        public IBinaryOutput this[HSREL8Output port] => GetOutput((int)port);
    }
}