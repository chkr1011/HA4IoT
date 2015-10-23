namespace HA4IoT.Contracts.Hardware
{
    public interface IPortExpanderDriver
    {
        int StateSize { get; }

        void Write(byte[] state);

        byte[] Read();
    }
}
