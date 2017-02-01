using System;

namespace HA4IoT.Contracts.Adapters
{
    public interface ISensorAdapter
    {
        event EventHandler<SensorAdapterValueChangedEventArgs> ValueChanged;
    }
}
