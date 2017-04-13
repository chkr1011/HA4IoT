namespace HA4IoT.Contracts.Hardware
{
    public interface IBinaryOutput
    {
        void Write(BinaryState state, bool commit = true);

        BinaryState Read();

        IBinaryOutput WithInvertedState(bool isInverted = true);
    }
}
