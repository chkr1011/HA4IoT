using System.Threading.Tasks;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Adapters
{
    public interface IBinaryOutputAdapter
    {
        Task SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters);
    }
}
