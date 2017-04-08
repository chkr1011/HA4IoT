using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Adapters
{
    public interface ILampAdapter
    {
        bool SupportsColor { get; }

        int ColorResolutionBits { get; }

        void SetState(AdapterPowerState powerState, AdapterColor color, params IHardwareParameter[] hardwareParameters);
    }
}
