using System;

namespace HA4IoT.Contracts.Services.OutdoorTemperature
{
    public class OutdoorTemperatureFetchedEventArgs : EventArgs
    {
        public OutdoorTemperatureFetchedEventArgs(float value)
        {
            OutdoorTemperature = value;
        }

        public float OutdoorTemperature { get; }
    }
}
