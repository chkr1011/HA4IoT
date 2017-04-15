namespace HA4IoT.Contracts.Hardware
{
    public interface IBinaryOutput
    {
        BinaryState Read();

        void Write(BinaryState state, WriteBinaryStateMode mode = WriteBinaryStateMode.Commit);

        IBinaryOutput WithInvertedState(bool isInverted = true);
    }
}
