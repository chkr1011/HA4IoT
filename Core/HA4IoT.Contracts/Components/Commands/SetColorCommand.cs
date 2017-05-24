namespace HA4IoT.Contracts.Components.Commands
{
    public class SetColorCommand : ICommand
    {
        public double Hue { get; set; }

        public double Saturation { get; set; }

        public double Value { get; set; }
    }
}
