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

        public bool InvertValue { get; set; }
        
        public void Write(BinaryState state, bool commit = true)
        {
            state = CoerceState(state);

            // TODO: Implement animations here.
            foreach (var output in _outputs)
            {
                output.Write(state, false);
            }

            if (commit)
            {
                foreach (var output in _outputs)
                {
                    output.Write(state);
                }
            }

            _state = state;
        }

        public BinaryState Read()
        {
            return CoerceState(_state);
        }

        public IBinaryOutput WithInvertedState(bool value = true)
        {
            InvertValue = value;
            return this;
        }

        public LogicalBinaryOutput WithOutput(IBinaryOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            _outputs.Add(output);
            return this;
        }

        private BinaryState CoerceState(BinaryState state)
        {
            if (InvertValue)
            {
                return state == BinaryState.High ? BinaryState.Low : BinaryState.High;
            }

            return state;
        }
    }
}
