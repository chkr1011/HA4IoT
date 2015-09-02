using CK.HomeAutomation.Core;

namespace CK.HomeAutomation.Hardware
{
    public interface IInputController
    {
        IBinaryInput GetInput(int number);
    }
}
