using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Adapters
{
    public interface IStateMachineStateAdapter
    {
        void Activate(params IHardwareParameter[] parameters);

        void Deactivate(params IHardwareParameter[] parameters);
    }
}
