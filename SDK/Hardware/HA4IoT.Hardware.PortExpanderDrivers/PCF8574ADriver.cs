using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.PortExpanderDrivers
{
    public class PCF8574ADriver : PCF8574Driver
    { 
        public PCF8574ADriver(I2CSlaveAddress address, II2CBus i2CBus) : base(address, i2CBus)
        {
        }
    }
}