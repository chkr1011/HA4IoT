using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.I2C.I2CPortExpanderDrivers
{
    public class PCF8574ADriver : PCF8574Driver
    { 
        public PCF8574ADriver(I2CSlaveAddress address, II2CBusService i2CBus) 
            : base(address, i2CBus)
        {
        }
    }
}