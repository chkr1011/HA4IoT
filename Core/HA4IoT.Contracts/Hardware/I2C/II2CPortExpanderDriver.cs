namespace HA4IoT.Contracts.Hardware.I2C
{
    public interface II2CPortExpanderDriver
    {
        int StateSize { get; }

        void Write(byte[] state);

        byte[] Read();
    }
}
