using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Hardware.PortExpanderDrivers;
using HA4IoT.Networking;

namespace HA4IoT.Hardware.CCTools
{
    public class HSRT16 : CCToolsBoardBase, IBinaryOutputController
    {
        public HSRT16(DeviceId id, I2CSlaveAddress address, II2CBus i2CBus, IHttpRequestController httpApi, ILogger logger)
            : base(id, new MAX7311Driver(address, i2CBus), httpApi, logger)
        {
            SetState(new byte[] { 0x00, 0x00 });
            CommitChanges(true);
        }

        public IBinaryOutput GetOutput(int number)
        {
            if (number < 0 || number > 15) throw new ArgumentOutOfRangeException(nameof(number));

            return GetPort(number);
        }

        public IBinaryOutput this[HSRT16Pin pin] => GetOutput((int) pin);
    }
}