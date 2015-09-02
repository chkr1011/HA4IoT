using System;

namespace CK.HomeAutomation.Core
{
    public interface IBinaryInput
    {
        event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        BinaryState Read();

        IBinaryInput WithInvertedState();
    }
}