using System;

namespace HA4IoT.Contracts.Hardware
{
    public static class BinaryOutputExtensions
    {
        public static IBinaryOutput WithInvertedState(this IBinaryOutput binaryOutput, bool isInverted = true)
        {
            if (binaryOutput == null) throw new ArgumentNullException(nameof(binaryOutput));

            binaryOutput.IsStateInverted = true;
            return binaryOutput;
        }
    }
}
