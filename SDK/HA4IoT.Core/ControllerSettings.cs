using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Core.Settings;
using HA4IoT.Core.Settings;

namespace HA4IoT.Core
{
    public class ControllerSettings : SettingsContainer, IControllerSettings
    {
        public ControllerSettings(string filename) 
            : base(filename)
        {
            Name = new Setting<string>("HA4IoT Controller");
            Description = new Setting<string>("The HA4IoT Controller");
        }

        public ISetting<string> Name { get; }

        public ISetting<string> Description { get; }       
    }
}
