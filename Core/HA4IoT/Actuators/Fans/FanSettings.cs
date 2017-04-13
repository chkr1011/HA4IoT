using HA4IoT.Contracts.Components;

namespace HA4IoT.Actuators.Fans
{
    public class FanSettings : ComponentSettings
    {
        public int DefaultActiveLevel { get; set; } = 1;
    }
}
