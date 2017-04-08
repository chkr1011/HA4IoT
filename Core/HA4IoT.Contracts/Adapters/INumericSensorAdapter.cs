using System;

namespace HA4IoT.Contracts.Adapters
{
    public interface INumericSensorAdapter
    {
        event EventHandler<NumericSensorAdapterValueChangedEventArgs> ValueChanged;

        void Refresh();
    }
}
