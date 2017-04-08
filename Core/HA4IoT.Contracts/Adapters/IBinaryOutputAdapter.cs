using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Adapters
{
    public interface IBinaryOutputAdapter
    {
        void SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters);
    }
}
