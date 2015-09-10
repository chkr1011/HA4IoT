namespace CK.HomeAutomation.Hardware
{
    public interface IBinaryOutput
    {
        void Write(BinaryState state, bool commit = true);

        BinaryState Read();

        IBinaryOutput WithInvertedState();
    }
}
