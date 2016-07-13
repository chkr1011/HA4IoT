namespace HA4IoT.Contracts.Services.Weather
{
    public interface IOutdoorHumidityService : IService
    {
        float GetOutdoorHumidity();
    }
}
