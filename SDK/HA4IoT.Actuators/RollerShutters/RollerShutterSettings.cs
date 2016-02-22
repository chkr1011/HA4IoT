using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Contracts.Logging;
using HA4IoT.Core.Settings;

namespace HA4IoT.Actuators
{
    public class RollerShutterSettings : ActuatorSettings, IRollerShutterSettings
    {
        public RollerShutterSettings(ActuatorId actuatorId, ILogger logger) : base(actuatorId, logger)
        {
            MaxPosition = new Setting<int>(20000);
        }
        
        public ISetting<int> MaxPosition { get; }
    }
}
