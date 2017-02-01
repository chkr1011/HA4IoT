using System;
using HA4IoT.Contracts.Adapters;

namespace HA4IoT.Tests.Mockups
{
    public class TestSensorAdapter : ISensorAdapter
    {
        public event EventHandler<SensorAdapterValueChangedEventArgs> ValueChanged;

        public void UpdateValue(float value)
        {
            ValueChanged?.Invoke(this, new SensorAdapterValueChangedEventArgs(value));
        }
    }
}
