namespace CK.HomeAutomation.Hardware
{
    public interface IDeviceDriver
    {
        int StateSize { get; }

        void Write(byte[] state);

        byte[] Read();
    }
}
