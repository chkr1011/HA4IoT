using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.PortExpanderDrivers
{
    public class PCA9555Driver : MAX7311Driver
    {
        public PCA9555Driver(I2CSlaveAddress address, II2CBus i2CBus) 
            : base(address, i2CBus)
        {
        }
    }
}