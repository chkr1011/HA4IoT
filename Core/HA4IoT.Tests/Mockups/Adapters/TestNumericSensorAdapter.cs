using System;
using HA4IoT.Contracts.Adapters;

namespace HA4IoT.Tests.Mockups.Adapters
{
    public class TestNumericSensorAdapter : INumericSensorAdapter
    {
        public event EventHandler<NumericSensorAdapterValueChangedEventArgs> ValueChanged;

        public void Refresh()
        {
        }

        public void UpdateValue(float? value)
        {
            ValueChanged?.Invoke(this, new NumericSensorAdapterValueChangedEventArgs(value));
        }
    }
}
