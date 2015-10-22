using System;

namespace HA4IoT.Hardware
{
    public interface IBinaryInput
    {
        event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        BinaryState Read();

        IBinaryInput WithInvertedState();
    }
}