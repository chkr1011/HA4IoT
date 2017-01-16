using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Actuators
{
    public interface IRollerShutterAdapter
    {
        void StartMoveUp(params IHardwareParameter[] parameters);

        void Stop(params IHardwareParameter[] parameters);

        void StartMoveDown(params IHardwareParameter[] parameters);
    }
}
