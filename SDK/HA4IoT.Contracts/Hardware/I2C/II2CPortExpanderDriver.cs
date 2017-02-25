namespace HA4IoT.Contracts.Hardware.I2C
{
    public interface I2CIPortExpanderDriver
    {
        int StateSize { get; }

        void Write(byte[] state);

        byte[] Read();
    }
}
