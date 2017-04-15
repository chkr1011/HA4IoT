using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Actuators.StateMachines
{
    public class PendingBinaryOutputState
    {
        public IBinaryOutput BinaryOutput { get; set; }

        public BinaryState State { get; set; }

        public void Execute(WriteBinaryStateMode mode)
        {
            BinaryOutput?.Write(State, mode);
        }
    }
}
