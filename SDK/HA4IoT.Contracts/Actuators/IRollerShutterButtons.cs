namespace HA4IoT.Contracts.Actuators
{
    public interface IRollerShutterButtons : IActuator
    {
        IButton Up { get; }
        IButton Down { get; }
    }
}
