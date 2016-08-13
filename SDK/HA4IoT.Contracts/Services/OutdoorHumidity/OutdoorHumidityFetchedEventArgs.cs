using System;

namespace HA4IoT.Contracts.Services.OutdoorHumidity
{
    public class OutdoorHumidityFetchedEventArgs : EventArgs
    {
        public OutdoorHumidityFetchedEventArgs(float value)
        {
            OutdoorHumidity = value;
        }

        public float OutdoorHumidity { get; }
    }
}
