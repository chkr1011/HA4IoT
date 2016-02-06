using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.I2CHardwareBridge
{
    public abstract class I2CHardwareBridgeCommand
    {
        public abstract void Execute(II2CDevice i2CDevice);
    }
}
