using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Tests.Mockups
{
    public class TestI2CBus : II2CBus
    {
        public TestI2CBus(DeviceId id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public DeviceId Id { get; }

        public void HandleApiCommand(IApiContext apiContext)
        {
        }

        public void HandleApiRequest(IApiContext apiContext)
        {
        }

        public void Execute(I2CSlaveAddress address, Action<II2CDevice> action, bool useCache = true)
        {
            
        }
    }
}
