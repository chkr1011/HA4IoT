namespace HA4IoT.Contracts.Hardware
{
    public interface IBinaryOutput
    {
        bool IsStateInverted { get; set; }

        BinaryState Read();
        
        void Write(BinaryState state, WriteBinaryStateMode mode = WriteBinaryStateMode.Commit);
    }
}
