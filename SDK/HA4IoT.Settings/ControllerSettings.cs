using Windows.Data.Json;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Settings
{
    public class ControllerSettings
    {
        private readonly SettingsContainer _settingsContainer;

        public ControllerSettings()
        {
            _settingsContainer = new SettingsContainer(StoragePath.WithFilename("ControllerConfiguration.json"));

            _settingsContainer.SetValue("Name", "HA4IoT Controller");
            _settingsContainer.SetValue("Description", "The HA4IoT controller which is responsible for this house.");
            _settingsContainer.SetValue("Language", "EN");

            // Ensure that the initial values are overridden with the already saved ones by loading the settings before saving.
            _settingsContainer.Load();
            _settingsContainer.Save();
        }

        public string Language
        {
            get { return _settingsContainer.GetString(nameof(Language)); }
            set
            {
                _settingsContainer.SetValue(nameof(Language), value);
                _settingsContainer.Save();
            }
        }

        public string Name
        {
            get { return _settingsContainer.GetString(nameof(Name)); }
            set
            {
                _settingsContainer.SetValue(nameof(Name), value);
                _settingsContainer.Save();
            }
        }

        public string Description
        {
            get { return _settingsContainer.GetString(nameof(Description)); }
            set
            {
                _settingsContainer.SetValue(nameof(Description), value);
                _settingsContainer.Save();
            }
        }

        public IJsonValue Export()
        {
            return _settingsContainer.Export();
        }
    }
}
