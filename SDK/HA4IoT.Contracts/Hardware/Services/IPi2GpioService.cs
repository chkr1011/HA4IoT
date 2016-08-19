using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Hardware.Services
{
    public interface IPi2GpioService : IBinaryOutputController, IBinaryInputController, IService
    {
    }
}