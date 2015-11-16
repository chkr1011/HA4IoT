using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware.GenericIOBoard;
using HA4IoT.Hardware.PortExpanderDrivers;
using HA4IoT.Notifications;

namespace HA4IoT.Hardware.CCTools
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
            if (number < 0 || number > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(number));
            }

            return GetPort(number);
        }
    }
}