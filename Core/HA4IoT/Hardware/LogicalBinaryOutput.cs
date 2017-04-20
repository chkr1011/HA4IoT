using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware
{
    public class LogicalBinaryOutput : IBinaryOutput
    {
        private readonly List<IBinaryOutput> _outputs = new List<IBinaryOutput>();
        private BinaryState _state;

        public LogicalBinaryOutput(params IBinaryOutput[] outputs)
        {
            if (outputs == null) throw new ArgumentNullException(nameof(outputs));

            _outputs.AddRange(outputs);
        }

        public bool IsStateInverted { get; set; }
        
        public void Write(BinaryState state, WriteBinaryStateMode mode)
        {
            var effectiveState = CoerceState(state);

            // TODO: Implement animations here.
            foreach (var output in _outputs)
            {
                output.Write(effectiveState, WriteBinaryStateMode.NoCommit);
            }

            if (mode == WriteBinaryStateMode.Commit)
            {
                foreach (var output in _outputs)
                {
                    output.Write(effectiveState);
                }
            }

            _state = state;
        }

        public BinaryState Read()
        {
            return CoerceState(_state);
        }

        public LogicalBinaryOutput WithOutput(IBinaryOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            _outputs.Add(output);
            return this;
        }

        private BinaryState CoerceState(BinaryState state)
        {
            if (IsStateInverted)
            {
                return state == BinaryState.High ? BinaryState.Low : BinaryState.High;
            }

            return state;
        }
    }
}
