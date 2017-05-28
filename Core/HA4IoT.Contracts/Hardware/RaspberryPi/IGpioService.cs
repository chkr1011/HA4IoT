using HA4IoT.Contracts.Services;

namespace HA4IoT.Contracts.Hardware.RaspberryPi
{
    public interface IGpioService : IService
    {
        IBinaryInput GetInput(int number, GpioPullMode pullMode, GpioInputMonitoringMode monitoringMode);

        IBinaryOutput GetOutput(int id);
    }
}