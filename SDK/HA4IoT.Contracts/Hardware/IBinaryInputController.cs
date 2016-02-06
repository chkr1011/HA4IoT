namespace HA4IoT.Contracts.Hardware
{
    public interface IBinaryInputController : IDevice
    {
        IBinaryInput GetInput(int number);
    }
}
