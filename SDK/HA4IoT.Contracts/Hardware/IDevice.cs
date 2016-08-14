using HA4IoT.Contracts.Api;

namespace HA4IoT.Contracts.Hardware
{
    public interface IDevice
    {
        DeviceId Id { get; }

        void HandleApiCall(IApiContext apiContext);
    }
}
