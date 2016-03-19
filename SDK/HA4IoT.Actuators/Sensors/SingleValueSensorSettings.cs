using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Core.Settings;

namespace HA4IoT.Actuators
{
    public class SingleValueSensorSettings : ActuatorSettings
    {
        public SingleValueSensorSettings(ActuatorId actuatorId, float defaultMinDelta) 
            : base(actuatorId)
        {
            MinDelta = new Setting<float>(defaultMinDelta);
        }

        public ISetting<float> MinDelta { get; }
    }
}
