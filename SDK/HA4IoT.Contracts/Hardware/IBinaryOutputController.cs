namespace HA4IoT.Contracts.Hardware
{
    public interface IBinaryOutputController
    {
        IBinaryOutput GetOutput(int id);
    }
}
