using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Hardware.PortExpanderDrivers;
using HA4IoT.Networking;

namespace HA4IoT.Hardware.CCTools
{
    public class HSREL8 : CCToolsBoardBase, IBinaryOutputController
    {
        public HSREL8(DeviceId id, I2CSlaveAddress i2CAddress, II2CBus i2CBus, IHttpRequestController httpApi, ILogger logger)
            : base(id, new MAX7311Driver(i2CAddress, i2CBus), httpApi, logger)
        {
            SetState(new byte[] { 0x00, 255 });
            CommitChanges(true);
        }

        public IBinaryOutput GetOutput(int number)
        {
            if (number < 0 || number > 15) throw new ArgumentOutOfRangeException(nameof(number));

            return GetPort(number);
        }

        public IBinaryOutput this[HSREL8Pin pin] => GetOutput((int) pin);
    }
}