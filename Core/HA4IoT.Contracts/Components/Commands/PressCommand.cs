using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Contracts.Components.Commands
{
    public class PressCommand : ICommand
    {
        public ButtonPressedDuration Duration { get; set; } = ButtonPressedDuration.Short;
    }
}
