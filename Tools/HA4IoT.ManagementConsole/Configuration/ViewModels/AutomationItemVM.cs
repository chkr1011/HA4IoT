using System;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels
{
    public class AutomationItemVM : ConfigurationItemVM
    {
        public AutomationItemVM(string id, string type, AutomationSettingsVM settings) : base(id)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            Type = type;
            Settings = settings;
        }

        public string Type { get; set; }

        public AutomationSettingsVM Settings { get; private set; }
    }
}
