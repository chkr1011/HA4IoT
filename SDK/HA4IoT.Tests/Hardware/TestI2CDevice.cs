using System;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.I2C;

namespace HA4IoT.Tests.Hardware
{
    public class TestI2CDevice : II2CDevice
    {
        public byte[] LastWrittenBytes { get; private set; }
        public byte[] BufferForNextRead { get; set; } = new byte[10];

        public void Write(byte[] writeBuffer)
        {
            LastWrittenBytes = writeBuffer;
        }

        public void Read(byte[] readBuffer)
        {
            if (BufferForNextRead == null)
            {
                throw new InvalidOperationException("The buffer for the next test I2C read operation is not set.");
            }

            Array.Copy(BufferForNextRead, readBuffer, readBuffer.Length);
        }

        public void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            LastWrittenBytes = writeBuffer;
            Array.Copy(BufferForNextRead, readBuffer, readBuffer.Length);
        }
    }
}
