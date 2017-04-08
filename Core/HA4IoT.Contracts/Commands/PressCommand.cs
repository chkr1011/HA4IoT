using HA4IoT.Contracts.Sensors;

namespace HA4IoT.Contracts.Commands
{
    public class PressCommand : ICommand
    {
        public ButtonPressedDuration Duration { get; set; } = ButtonPressedDuration.Short;
    }
}
