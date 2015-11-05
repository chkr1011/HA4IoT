namespace HA4IoT.Contracts.Hardware
{
    public interface IBinaryInputController
    {
        IBinaryInput GetInput(int number);
    }
}
