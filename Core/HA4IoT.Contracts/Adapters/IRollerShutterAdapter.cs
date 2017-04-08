using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Adapters
{
    public interface IRollerShutterAdapter
    {
        void SetState(AdapterRollerShutterState state, params IHardwareParameter[] parameters);
    }
}
