namespace HA4IoT.Contracts.Hardware
{
    public interface IBinaryOutputController : IDevice
    {
        IBinaryOutput GetOutput(int number);
    }
}
