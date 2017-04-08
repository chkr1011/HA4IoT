using HA4IoT.Contracts.Components;

namespace HA4IoT.Sensors
{
    public class SingleValueSensorSettings : ComponentSettings
    {
        public float MinDelta { get; set; } = 0.15F;
    }
}
