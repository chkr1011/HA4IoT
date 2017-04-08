using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Hardware.I2C.I2CPortExpanderDrivers
{
    public class PCA9555ADriver : PCA9555Driver
    {
        public PCA9555ADriver(I2CSlaveAddress address, II2CBusService i2CBus) 
            : base(address, i2CBus)
        {
        }
    }
}