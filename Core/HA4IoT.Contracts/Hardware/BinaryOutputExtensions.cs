using System;

namespace HA4IoT.Contracts.Hardware
{
    public static class BinaryOutputExtensions
    {
        public static IBinaryOutput WithInvertedState(this IBinaryOutput binaryOutput)
        {
            if (binaryOutput == null) throw new ArgumentNullException(nameof(binaryOutput));

            return new InvertedBinarOutput(binaryOutput);
        }
    }
}
