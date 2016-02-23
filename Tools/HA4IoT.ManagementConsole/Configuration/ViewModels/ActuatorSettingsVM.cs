using System.Collections.ObjectModel;
using HA4IoT.ManagementConsole.Configuration.ViewModels.Settings;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels
{
    public class ActuatorSettingsVM
    {
        public ActuatorSettingsVM()
        {
            AppSettings = new ActuatorAppSettingsVM();
        }

        public bool IsEnabled { get; set; }

        public ActuatorAppSettingsVM AppSettings { get; private set; }
    }
}
