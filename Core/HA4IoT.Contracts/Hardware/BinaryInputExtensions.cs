using System;

namespace HA4IoT.Contracts.Hardware
{
    public static class BinaryInputExtensions
    {
        public static IBinaryInput WithInvertedState(this IBinaryInput binaryInput)
        {
            if (binaryInput == null) throw new ArgumentNullException(nameof(binaryInput));

            return new InvertedBinaryInput(binaryInput);
        }
    }
}
