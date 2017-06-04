using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Environment;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapResponseParser
    {
        public float Temperature { get; private set; }

        public float Humidity { get; private set; }

        public TimeSpan Sunrise { get; private set; }

        public TimeSpan Sunset { get; private set; }

        public WeatherCondition Condition { get; private set; }

        public int ConditionCode { get; private set; }

        public void Parse(string source)
        {
            var data = JsonObject.Parse(source);

            var main = data.GetNamedObject("main");
            Temperature = (float)main.GetNamedNumber("temp", 0);
            Humidity = (float)main.GetNamedNumber("humidity", 0);

            var sys = data.GetNamedObject("sys");
            var sunriseValue = sys.GetNamedNumber("sunrise", 0);
            var sunsetValue = sys.GetNamedNumber("sunset", 0);
            Sunrise = UnixTimeStampToDateTime(sunriseValue).TimeOfDay;
            Sunset = UnixTimeStampToDateTime(sunsetValue).TimeOfDay;

            var weather = data.GetNamedArray("weather");
            var weatherId = (int)weather.GetObjectAt(0).GetNamedNumber("id");
            ConditionCode = weatherId;
            Condition = OpenWeatherMapWeatherConditionParser.Parse(ConditionCode);
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var buffer = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return buffer.AddSeconds(unixTimeStamp).ToLocalTime();
        }
    }
}
