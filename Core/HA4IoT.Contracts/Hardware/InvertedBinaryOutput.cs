using System;

namespace HA4IoT.Contracts.Hardware
{
    public class InvertedBinarOutput : InvertedBinaryInput, IBinaryOutput
    {
        private readonly IBinaryOutput _binaryOutput;

        public InvertedBinarOutput(IBinaryOutput binaryOutput) 
            : base(binaryOutput)
        {
            _binaryOutput = binaryOutput ?? throw new ArgumentNullException(nameof(binaryOutput));
        }
        
        public void Write(BinaryState state, WriteBinaryStateMode mode = WriteBinaryStateMode.Commit)
        {
            _binaryOutput.Write(CoerceState(state), mode);
        }
    }
}
