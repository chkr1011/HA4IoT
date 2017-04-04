using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Hardware.Services
{
    public interface IGpioService : IBinaryOutputController, IBinaryInputController, IService
    {
    }
}