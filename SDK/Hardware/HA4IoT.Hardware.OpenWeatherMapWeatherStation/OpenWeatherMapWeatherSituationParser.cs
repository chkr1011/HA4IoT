using System;
using Windows.Data.Json;
using HA4IoT.Contracts.WeatherStation;

namespace HA4IoT.Hardware.OpenWeatherMapWeatherStation
{
    public class OpenWeatherMapWeatherSituationParser
    {
        public WeatherSituation Parse(JsonValue id)
        {
            switch (Convert.ToInt32(id.GetNumber()))
            {
                case 200: return WeatherSituation.ThunderstormWithLightRain;
                case 201: return WeatherSituation.ThunderstormWithRain;
                case 202: return WeatherSituation.ThunderstormWithHeavyRain;
                case 210: return WeatherSituation.LightThunderstorm;
                case 211: return WeatherSituation.Thunderstorm;
                case 212: return WeatherSituation.HeavyThunderstorm;
                case 221: return WeatherSituation.RaggedThunderstorm;
                case 230: return WeatherSituation.ThunderstormWithLightDrizzle;
                case 231: return WeatherSituation.ThunderstormWithDrizzle;
                case 232: return WeatherSituation.ThunderstormWithHeavyDrizzle;

                case 300: return WeatherSituation.LightIntensityDrizzle;
                case 301: return WeatherSituation.Drizzle;
                case 302: return WeatherSituation.HeavyIntensityDrizzle;

                    //TODO:
////310 light intensity drizzle rain     09d
////311 drizzle rain     09d
////312 heavy intensity drizzle rain     09d
////313 shower rain and drizzle  09d
////314 heavy shower rain and drizzle    09d
////321 shower drizzle   09d

                case 500: return WeatherSituation.LightRain;
                case 501: return WeatherSituation.ModerateRain;
                case 502: return WeatherSituation.HeavyIntensityRain;
                case 503: return WeatherSituation.VeryHeavyRain;
                case 504: return WeatherSituation.ExtremeRain;
                case 511: return WeatherSituation.FreezingRain;
                case 520: return WeatherSituation.LightIntensityShowerRain;
                case 521: return WeatherSituation.ShowerRain;
                case 522: return WeatherSituation.HeavyIntensityShowerRain;
                case 531: return WeatherSituation.RaggedShowerRain;

                case 600: return WeatherSituation.LightSnow;
                case 601: return WeatherSituation.Snow;
                case 602: return WeatherSituation.HeavySnow;
                case 611: return WeatherSituation.Sleet;
                case 612: return WeatherSituation.ShowerSleet;
                case 615: return WeatherSituation.LightRainAndSnow;
                case 616: return WeatherSituation.RainAndSnow;
                case 620: return WeatherSituation.LightShowerSnow;
                case 621: return WeatherSituation.ShowerSnow;
                case 622: return WeatherSituation.HeavyShowerSnow;

                case 701: return WeatherSituation.Mist;

                case 800: return WeatherSituation.SkyIsClear;
                case 801: return WeatherSituation.FewClouds;
                case 802: return WeatherSituation.ScatteredClouds;
                case 803: return WeatherSituation.BrokenClouds;
                case 804: return WeatherSituation.OvercastClouds;

                default:
                    {
                        return WeatherSituation.Unknown;
                    }
            }
        }
    }
}
