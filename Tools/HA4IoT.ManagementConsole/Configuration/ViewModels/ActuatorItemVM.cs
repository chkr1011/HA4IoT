using System;
using System.Collections.ObjectModel;
using HA4IoT.ManagementConsole.Configuration.ViewModels.Settings;

namespace HA4IoT.ManagementConsole.Configuration.ViewModels
{
    public class ActuatorItemVM : ConfigurationItemVM
    {
        public ActuatorItemVM(string id, string type, ActuatorSettingsVM settings) : base(id)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            Type = type;
            Settings = settings;
            ExtendedSettings = new ObservableCollection<SettingBaseVM>();
        }

        public string Type { get; set; }

        public ActuatorSettingsVM Settings { get; private set; }

        public ObservableCollection<SettingBaseVM> ExtendedSettings { get; private set; }
    }
}
