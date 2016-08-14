using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Tests.Mockups
{
    public class TestI2CBusService : ServiceBase, II2CBusService
    {
        public TestI2CDevice I2CDevice { get; } = new TestI2CDevice();

        public I2CSlaveAddress LastUsedI2CSlaveAddress { get; private set; }

        public void Execute(I2CSlaveAddress address, Action<II2CDevice> action, bool useCache = true)
        {
            LastUsedI2CSlaveAddress = address;
            action(I2CDevice);
        }
    }
}
