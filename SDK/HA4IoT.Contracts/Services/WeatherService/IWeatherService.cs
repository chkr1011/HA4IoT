namespace HA4IoT.Contracts.Services.WeatherService
{
    public interface IWeatherService : IService
    {
        // TODO: Consider split into ExternalTemperatureService, ExternalHumidityService, ExternalWeatherSituationService

        WeatherSituation GetSituation();

        float GetTemperature();

        float GetHumidity();
    }
}