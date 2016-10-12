using HA4IoT.Contracts.Components;

namespace HA4IoT.Services.Status
{
    public class ActuatorStatus
    {
        public ComponentId Id { get; set; }

        public string Caption { get; set; }
    }
}
