using System;
using CK.HomeAutomation.Core;

namespace CK.HomeAutomation.Hardware
{
    public class BinaryOutputWithState
    {
        public BinaryOutputWithState(IBinaryOutput output, BinaryState state)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            Output = output;
            State = state;
        }

        public IBinaryOutput Output { get; }

        public BinaryState State { get; }
    }
}
