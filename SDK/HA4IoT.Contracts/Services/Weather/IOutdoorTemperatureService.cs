namespace HA4IoT.Contracts.Services.Weather
{
    public interface IOutdoorTemperatureService : IService
    {
        float GetOutdoorTemperature();
    }
}
