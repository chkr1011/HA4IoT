namespace HA4IoT.Hardware
{
    public interface IPortExpanderDriver
    {
        int StateSize { get; }

        void Write(byte[] state);

        byte[] Read();
    }
}
