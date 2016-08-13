using System;

namespace HA4IoT.Contracts.Services.Weather
{
    public class WeatherFetchedEventArgs : EventArgs
    {
        public WeatherFetchedEventArgs(Weather weather)
        {
            Weather = weather;
        }

        public Weather Weather { get; }
    }
}
