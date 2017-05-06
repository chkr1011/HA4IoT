using System;

namespace HA4IoT.Contracts.Hardware
{
    public interface IBinaryInput
    {
        event EventHandler<BinaryStateChangedEventArgs> StateChanged;

        bool IsStateInverted { set; }

        BinaryState Read();
    }
}