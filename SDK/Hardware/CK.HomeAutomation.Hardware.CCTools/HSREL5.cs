using CK.HomeAutomation.Hardware.GenericIOBoard;
using CK.HomeAutomation.Hardware.PortExpanderDrivers;
using CK.HomeAutomation.Notifications;

namespace CK.HomeAutomation.Hardware.CCTools
{
    public class HSREL5 : IOBoardController, IOutputController
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