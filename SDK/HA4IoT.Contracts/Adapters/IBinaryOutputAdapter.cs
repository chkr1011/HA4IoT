using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Adapters
{
    public interface IBinaryOutputAdapter
    {
        void TurnOn(params IHardwareParameter[] parameters);

        void TurnOff(params IHardwareParameter[] parameters);
    }
}
