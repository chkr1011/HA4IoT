using System;

namespace HA4IoT.Contracts.Components.Adapters
{
    public interface INumericSensorAdapter
    {
        event EventHandler<NumericSensorAdapterValueChangedEventArgs> ValueChanged;

        void Refresh();
    }
}
