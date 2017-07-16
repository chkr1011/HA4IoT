using System.Threading.Tasks;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Components.Adapters
{
    public interface IRollerShutterAdapter
    {
        Task SetState(AdapterRollerShutterState state, params IHardwareParameter[] parameters);
    }
}
