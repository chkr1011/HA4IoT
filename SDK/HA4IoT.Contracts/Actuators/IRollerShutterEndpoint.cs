using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Contracts.Actuators
{
    public interface IRollerShutterEndpoint
    {
        void StartMoveUp(params IHardwareParameter[] parameters);

        void Stop(params IHardwareParameter[] parameters);

        void StartMoveDown(params IHardwareParameter[] parameters);
    }
}
