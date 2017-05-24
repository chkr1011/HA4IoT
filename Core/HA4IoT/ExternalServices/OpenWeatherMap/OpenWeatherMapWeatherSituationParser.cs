using HA4IoT.Contracts.Environment;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public static class OpenWeatherMapWeatherSituationParser
    {
        public static WeatherCondition Parse(int id)
        {
            switch (id)
            {
                case 800: return WeatherCondition.ClearSky;
                case 801: return WeatherCondition.FewClouds;
                case 802: return WeatherCondition.ScatteredClouds;

                case 803:
                case 804:
                    {
                        return WeatherCondition.BrokenClouds;
                    }

                case 906: return WeatherCondition.Hail;

                case 960:
                case 961:
                case 962:
                    {
                        return WeatherCondition.Storm;
                    }
            }

            if (id >= 200 && id <= 299)
            {
                return WeatherCondition.Thunderstorm;
            }

            if (id >= 300 && id <= 399)
            {
                return WeatherCondition.ShowerRain;
            }

            if (id >= 500 && id <= 599)
            {
                return WeatherCondition.Rain;
            }

            if (id >= 600 && id <= 699)
            {
                return WeatherCondition.Snow;
            }

            if (id >= 700 && id <= 799)
            {
                return WeatherCondition.Mist;
            }

            return WeatherCondition.Unknown;
        }
    }
}
