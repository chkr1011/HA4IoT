namespace HA4IoT.Contracts.Actuators
{
    public class CasementStateChangedEventArgs : ValueChangedEventArgs<CasementState>
    {
        public CasementStateChangedEventArgs(CasementState oldValue, CasementState newValue) 
            : base(oldValue, newValue)
        {
        }
    }
}
