using System;
using System.Threading.Tasks;
using System.Windows.Input;
using HA4IoT.ManagementConsole.Controller;
using HA4IoT.ManagementConsole.Core;
using HA4IoT.ManagementConsole.Json;
using Newtonsoft.Json.Linq;

namespace HA4IoT.ManagementConsole.WeatherStation.Model
{
    public class WeatherStationTabVM
    {
        private readonly ControllerClient _controllerClient;

        public WeatherStationTabVM(ControllerClient controllerClient)
        {
            if (controllerClient == null) throw new ArgumentNullException(nameof(controllerClient));

            _controllerClient = controllerClient;

            ApiKey = new PropertyVM<string>(string.Empty);
            Latitude = new PropertyVM<string>(string.Empty);
            Longitude = new PropertyVM<string>(string.Empty);

            RefreshCommand = new AsyncDelegateCommand(Refresh);
            SaveCommand = new AsyncDelegateCommand(Save);
        }

        public PropertyVM<string> Longitude { get; set; }

        public PropertyVM<string> Latitude { get; }

        public PropertyVM<string> ApiKey { get; }

        public ICommand RefreshCommand { get; }

        public ICommand SaveCommand { get; }

        private async Task Refresh()
        {
            JObject configuration = await _controllerClient.GetStorageJsonFile("OpenWeatherMapConfiguration.json");

            ApiKey.Value = configuration.GetNamedString("appID", string.Empty);
            Latitude.Value = configuration.GetNamedString("lat", string.Empty);
            Longitude.Value = configuration.GetNamedString("lon", string.Empty);
        }

        private async Task Save()
        {
            var configuration = new JObject();
            configuration.SetNamedString("appID", ApiKey.Value);
            configuration.SetNamedString("lat", Latitude.Value);
            configuration.SetNamedString("lon", Longitude.Value);

            await _controllerClient.PostStorageJsonFile("OpenWeatherMapConfiguration.json", configuration);
        }
    }
}
