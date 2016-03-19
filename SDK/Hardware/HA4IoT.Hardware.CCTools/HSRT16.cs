using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Hardware.PortExpanderDrivers;

namespace HA4IoT.Hardware.CCTools
{
    public class HSRT16 : CCToolsBoardBase, IBinaryOutputController
    {
        public HSRT16(DeviceId id, I2CSlaveAddress address, II2CBus i2CBus, IApiController apiController)
            : base(id, new MAX7311Driver(address, i2CBus), apiController)
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