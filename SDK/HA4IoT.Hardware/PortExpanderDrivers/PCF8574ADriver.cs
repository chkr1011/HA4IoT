using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Hardware.PortExpanderDrivers
{
    public class PCF8574ADriver : PCF8574Driver
    { 
        public PCF8574ADriver(I2CSlaveAddress address, II2CBusService i2CBus) 
            : base(address, i2CBus)
        {
        }
    }
}