using System;

namespace HA4IoT.Contracts.Hardware
{
    public static class BinaryInputExtensions
    {
        public static IBinaryInput WithInvertedState(this IBinaryInput binaryInput, bool isInverted = true)
        {
            if (binaryInput == null) throw new ArgumentNullException(nameof(binaryInput));

            binaryInput.IsStateInverted = true;
            return binaryInput;
        }
    }
}
