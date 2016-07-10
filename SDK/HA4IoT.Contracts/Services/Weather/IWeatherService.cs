namespace HA4IoT.Contracts.Services.Weather
{
    public interface IWeatherService : IService
    {
        WeatherSituation GetWeather();
    }
}
