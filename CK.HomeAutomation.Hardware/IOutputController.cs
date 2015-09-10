namespace CK.HomeAutomation.Hardware
{
    public interface IOutputController
    {
        IBinaryOutput GetOutput(int number);
    }
}
