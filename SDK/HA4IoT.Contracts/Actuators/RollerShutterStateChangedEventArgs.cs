namespace HA4IoT.Contracts.Actuators
{
    public class RollerShutterStateChangedEventArgs : ValueChangedEventArgs<RollerShutterState>
    {
        public RollerShutterStateChangedEventArgs(RollerShutterState oldValue, RollerShutterState newValue) : base(oldValue, newValue)
        {
        }
    }
}
