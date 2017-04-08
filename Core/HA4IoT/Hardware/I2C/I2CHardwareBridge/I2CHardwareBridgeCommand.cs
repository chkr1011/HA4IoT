using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Hardware.I2C.I2CHardwareBridge
{
    public abstract class I2CHardwareBridgeCommand
    {
        public abstract void Execute(II2CDevice i2CDevice);
    }
}
