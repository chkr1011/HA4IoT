using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.PortExpanderDrivers
{
    public class PCA9555ADriver : PCA9555Driver
    {
        public PCA9555ADriver(I2CSlaveAddress address, II2CBus i2CBus) 
            : base(address, i2CBus)
        {
        }
    }
}