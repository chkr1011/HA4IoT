using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Actuators
{
    public interface IBinaryOutputComponentAdapter
    {
        void TurnOn(params IHardwareParameter[] parameters);

        void TurnOff(params IHardwareParameter[] parameters);
    }
}
