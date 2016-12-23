using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Hardware.PortExpanderDrivers
{
    public class PCA9555ADriver : PCA9555Driver
    {
        public PCA9555ADriver(I2CSlaveAddress address, II2CBusService i2CBus) 
            : base(address, i2CBus)
        {
        }
    }
}