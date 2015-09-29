using System;

namespace CK.HomeAutomation.Hardware
{
    public interface IBinaryInput
    {
        event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        BinaryState Read();

        IBinaryInput WithInvertedState();
    }
}