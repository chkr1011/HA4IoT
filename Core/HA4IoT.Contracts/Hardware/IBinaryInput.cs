using System;

namespace HA4IoT.Contracts.Hardware
{
    public interface IBinaryInput
    {
        event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        BinaryState Read();
    }
}