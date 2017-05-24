namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapServiceSettings
    {
        public bool IsEnabled { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string AppId { get; set; }

        public bool UseTemperature { get; set; } = true;

        public bool UseHumidity { get; set; } = true;

        public bool UseSunriseSunset { get; set; } = true;

        public bool UseWeather { get; set; } = true;
    }
}
