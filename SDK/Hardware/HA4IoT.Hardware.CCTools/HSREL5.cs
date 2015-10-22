using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware.GenericIOBoard;
using HA4IoT.Hardware.PortExpanderDrivers;
using HA4IoT.Notifications;

namespace HA4IoT.Hardware.CCTools
{
    public class HSREL5 : IOBoardController, IBinaryOutputController
    {
        public HSREL5(string id, int address, II2cBusAccessor bus, INotificationHandler notificationHandler)
            : base(id, new PCF8574Driver(address, bus), notificationHandler)
        {
            // Ensure that all relays are off by default. The first 5 ports are hardware inverted! The other ports are not inverted but the
            // connected relays are inverted.
            SetState(new byte[] { 0xFF });
            CommitChanges(true);
        }

        public IBinaryOutput GetOutput(int number)
        {
            var port = GetPort(number);
            if (number <= 4)
            {
                // The 4 relays have an hardware inverter. Invert here again to ensure that high means "on".
                return port.WithInvertedState();
            }

            return port;
        }
    }
}