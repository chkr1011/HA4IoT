namespace HA4IoT.Contracts.Actuators
{
    public interface IRollerShutter : IStateMachine
    {
        bool IsClosed { get; }
    }
}
