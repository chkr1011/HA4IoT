namespace HA4IoT.Contracts.Hardware
{
    public interface IBinaryOutput : IBinaryInput
    {
        void Write(BinaryState state, WriteBinaryStateMode mode = WriteBinaryStateMode.Commit);
    }
}
