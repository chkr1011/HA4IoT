using System;
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

        public void Execute(I2CSlaveAddress address, Action<II2CDevice> action, bool useCache = true)
        {
            
        }
    }
}
