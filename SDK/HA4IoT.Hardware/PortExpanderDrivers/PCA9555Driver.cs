using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Hardware.PortExpanderDrivers
{
    public class PCA9555Driver : MAX7311Driver
    {
        public PCA9555Driver(I2CSlaveAddress address, II2CBusService i2CBus) 
            : base(address, i2CBus)
        {
        }
    }
}