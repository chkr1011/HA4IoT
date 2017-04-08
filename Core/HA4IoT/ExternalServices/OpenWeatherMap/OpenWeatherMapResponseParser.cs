using System;
using System.Linq;
using Windows.Data.Json;
using HA4IoT.Contracts.Services.Weather;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapResponseParser
    {
        public float Temperature { get; private set; }

        public float Humidity { get; private set; }

        public TimeSpan Sunrise { get; private set; }

        public TimeSpan Sunset { get; private set; }

        public Weather Weather { get; private set; }

        public void Parse(string source)
        {
            var data = JsonObject.Parse(source);

            var sys = data.GetNamedObject("sys");
            var main = data.GetNamedObject("main");
            var weather = data.GetNamedArray("weather");

            var sunriseValue = sys.GetNamedNumber("sunrise", 0);
            Sunrise = UnixTimeStampToDateTime(sunriseValue).TimeOfDay;

            var sunsetValue = sys.GetNamedNumber("sunset", 0);
            Sunset = UnixTimeStampToDateTime(sunsetValue).TimeOfDay;

            var situationValue = weather.First().GetObject().GetNamedValue("id");
            Weather = new OpenWeatherMapWeatherSituationParser().Parse(situationValue);

            Temperature = (float) main.GetNamedNumber("temp", 0);
            Humidity = (float) main.GetNamedNumber("humidity", 0);
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var buffer = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return buffer.AddSeconds(unixTimeStamp).ToLocalTime();
        }
    }
}
