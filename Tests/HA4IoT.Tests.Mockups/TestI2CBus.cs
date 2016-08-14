using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Tests.Mockups
{
    public class TestI2CBus : II2CBusService
    {
        public TestI2CBus(DeviceId deviceId)
        {
            Id = deviceId;
        }

        public DeviceId Id { get; }

        public TestI2CDevice I2CDevice { get; } = new TestI2CDevice();

        public I2CSlaveAddress LastUsedI2CSlaveAddress { get; private set; }

        public void HandleApiCommand(IApiContext apiContext)
        {
            throw new NotSupportedException();
        }

        public void HandleApiRequest(IApiContext apiContext)
        {
            throw new NotSupportedException();
        }

        public void Execute(I2CSlaveAddress address, Action<II2CDevice> action, bool useCache = true)
        {
            LastUsedI2CSlaveAddress = address;
            action(I2CDevice);
        }
    }
}
