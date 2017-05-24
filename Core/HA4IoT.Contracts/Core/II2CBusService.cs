using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Core
{
    public interface II2CBusService : IService
    {
        II2CTransferResult Write(I2CSlaveAddress address, byte[] buffer, bool useCache = true);

        II2CTransferResult Read(I2CSlaveAddress address, byte[] buffer, bool useCache = true);

        II2CTransferResult WriteRead(I2CSlaveAddress address, byte[] writeBuffer, byte[] readBuffer, bool useCache = true);
    }
}
