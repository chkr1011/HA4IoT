using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware
{
    public sealed class LogicalBinaryOutput : IBinaryOutput
    {
        private readonly List<IBinaryOutput> _outputs = new List<IBinaryOutput>();
        private BinaryState _state;

        public LogicalBinaryOutput(params IBinaryOutput[] outputs)
        {
            if (outputs == null) throw new ArgumentNullException(nameof(outputs));

            _outputs.AddRange(outputs);
        }

        public event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        public void Write(BinaryState state, WriteBinaryStateMode mode)
        {
            // TODO: Implement animations here.
            foreach (var output in _outputs)
            {
                output.Write(state, WriteBinaryStateMode.NoCommit);
            }

            if (mode == WriteBinaryStateMode.Commit)
            {
                foreach (var output in _outputs)
                {
                    output.Write(state);
                }
            }

            var oldState = _state;
            _state = state;

            StateChanged?.Invoke(this, new BinaryStateChangedEventArgs(oldState, _state));
        }

        public BinaryState Read()
        {
            return _state;
        }

        public LogicalBinaryOutput WithOutput(IBinaryOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            _outputs.Add(output);
            return this;
        }
    }
}
