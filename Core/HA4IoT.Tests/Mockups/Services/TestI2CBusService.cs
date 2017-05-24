using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Contracts.Services;
using HA4IoT.Hardware.I2C;

namespace HA4IoT.Tests.Mockups.Services
{
    public class TestI2CBusService : ServiceBase, II2CBusService
    {
        public I2CSlaveAddress LastUsedI2CSlaveAddress { get; private set; }

        public byte[] LastWrittenBytes { get; private set; }

        public byte[] BufferForNextRead { get; set; }

        public II2CTransferResult Write(I2CSlaveAddress address, byte[] buffer, bool useCache = true)
        {
            LastUsedI2CSlaveAddress = address;
            LastWrittenBytes = buffer;
            return new I2CTransferResult(I2CTransferStatus.FullTransfer, buffer.Length);
        }

        public II2CTransferResult Read(I2CSlaveAddress address, byte[] buffer, bool useCache = true)
        {
            LastUsedI2CSlaveAddress = address;
            Array.Copy(BufferForNextRead, buffer, buffer.Length);
            return new I2CTransferResult(I2CTransferStatus.FullTransfer, buffer.Length);
        }

        public II2CTransferResult WriteRead(I2CSlaveAddress address, byte[] writeBuffer, byte[] readBuffer, bool useCache = true)
        {
            LastUsedI2CSlaveAddress = address;
            LastWrittenBytes = writeBuffer;
            Array.Copy(BufferForNextRead, readBuffer, readBuffer.Length);
            return new I2CTransferResult(I2CTransferStatus.FullTransfer, writeBuffer.Length + readBuffer.Length);
        }
    }
}
