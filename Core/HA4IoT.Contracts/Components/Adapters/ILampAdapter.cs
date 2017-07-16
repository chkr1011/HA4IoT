using System.Threading.Tasks;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Components.Adapters
{
    public interface ILampAdapter
    {
        bool SupportsColor { get; }

        int ColorResolutionBits { get; }

        Task SetState(AdapterPowerState powerState, AdapterColor color, params IHardwareParameter[] hardwareParameters);
    }
}
