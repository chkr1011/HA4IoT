using System;

namespace HA4IoT.ManagementConsole.Home.ViewModels
{
    public class ActuatorItemVM : HomeItemVM
    {
        public ActuatorItemVM(string id, string type, ActuatorSettingsVM settings) : base(id)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            Type = type;
            Settings = settings;
        }

        public string Type { get; set; }

        public ActuatorSettingsVM Settings { get; private set; }
    }
}
