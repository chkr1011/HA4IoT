namespace HA4IoT.Contracts.Actuators
{
    public interface IRollerShutterEndpoint
    {
        void StartMoveUp(params IParameter[] parameters);

        void Stop(params IParameter[] parameters);

        void StartMoveDown(params IParameter[] parameters);
    }
}
