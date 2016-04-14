namespace HA4IoT.Contracts.Sensors
{
    public interface INumericValueSensor : ISensor
    {
        float GetCurrentNumericValue();
    }
}
