using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Adapters
{
    public interface IFanAdapter
    {
        int MaxLevel { get; }

        void SetState(int level, params IHardwareParameter[] parameters);
    }
}
