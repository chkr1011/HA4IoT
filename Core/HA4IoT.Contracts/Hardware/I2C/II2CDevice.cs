namespace HA4IoT.Contracts.Hardware.I2C
{
    public interface II2CDevice
    {
        void Write(byte[] writeBuffer);

        void Read(byte[] readBuffer);

        void WriteRead(byte[] writeBuffer, byte[] readBuffer);
    }
}
