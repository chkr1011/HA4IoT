using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.Drivers.I2CHardwareBridge
{
    public abstract class I2CHardwareBridgeCommand
    {
        public abstract void Execute(I2CSlaveAddress address, II2CBusService i2CBusService);
    }
}
