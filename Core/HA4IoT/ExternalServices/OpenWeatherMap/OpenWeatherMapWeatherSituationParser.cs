using System;
using Windows.Data.Json;
using HA4IoT.Contracts.Services.Weather;

namespace HA4IoT.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapWeatherSituationParser
    {
        public Weather Parse(JsonValue id)
        {
            switch (Convert.ToInt32(id.GetNumber()))
            {
                case 200: return Weather.ThunderstormWithLightRain;
                case 201: return Weather.ThunderstormWithRain;
                case 202: return Weather.ThunderstormWithHeavyRain;
                case 210: return Weather.LightThunderstorm;
                case 211: return Weather.Thunderstorm;
                case 212: return Weather.HeavyThunderstorm;
                case 221: return Weather.RaggedThunderstorm;
                case 230: return Weather.ThunderstormWithLightDrizzle;
                case 231: return Weather.ThunderstormWithDrizzle;
                case 232: return Weather.ThunderstormWithHeavyDrizzle;

                case 300: return Weather.LightIntensityDrizzle;
                case 301: return Weather.Drizzle;
                case 302: return Weather.HeavyIntensityDrizzle;

                    //TODO:
////310 light intensity drizzle rain     09d
////311 drizzle rain     09d
////312 heavy intensity drizzle rain     09d
////313 shower rain and drizzle  09d
////314 heavy shower rain and drizzle    09d
////321 shower drizzle   09d

                case 500: return Weather.LightRain;
                case 501: return Weather.ModerateRain;
                case 502: return Weather.HeavyIntensityRain;
                case 503: return Weather.VeryHeavyRain;
                case 504: return Weather.ExtremeRain;
                case 511: return Weather.FreezingRain;
                case 520: return Weather.LightIntensityShowerRain;
                case 521: return Weather.ShowerRain;
                case 522: return Weather.HeavyIntensityShowerRain;
                case 531: return Weather.RaggedShowerRain;

                case 600: return Weather.LightSnow;
                case 601: return Weather.Snow;
                case 602: return Weather.HeavySnow;
                case 611: return Weather.Sleet;
                case 612: return Weather.ShowerSleet;
                case 615: return Weather.LightRainAndSnow;
                case 616: return Weather.RainAndSnow;
                case 620: return Weather.LightShowerSnow;
                case 621: return Weather.ShowerSnow;
                case 622: return Weather.HeavyShowerSnow;

                case 701: return Weather.Mist;

                case 800: return Weather.SkyIsClear;
                case 801: return Weather.FewClouds;
                case 802: return Weather.ScatteredClouds;
                case 803: return Weather.BrokenClouds;
                case 804: return Weather.OvercastClouds;

                default:
                    {
                        return Weather.Unknown;
                    }
            }
        }
    }
}
