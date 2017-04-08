namespace HA4IoT.Contracts.Components.States
{
    public class PowerState : EnumBasedState<PowerStateValue>
    {
        public static readonly PowerState Off = new PowerState(PowerStateValue.Off);
        public static readonly PowerState On = new PowerState(PowerStateValue.On);

        public PowerState(PowerStateValue value) : base(value)
        {
        }
    }
}
