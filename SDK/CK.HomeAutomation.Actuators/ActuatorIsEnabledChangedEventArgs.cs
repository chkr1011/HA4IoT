namespace CK.HomeAutomation.Actuators
{
    public class ActuatorIsEnabledChangedEventArgs : ValueChangedEventArgsBase<bool>
    {
        public ActuatorIsEnabledChangedEventArgs(bool oldValue, bool newValue) : base(oldValue, newValue)
        {
        }
    }
}
