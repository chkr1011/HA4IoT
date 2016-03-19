using System.Windows.Input;
using HA4IoT.ManagementConsole.Core;

namespace HA4IoT.ManagementConsole.WeatherStationConfiguration.Model
{
    public class WeatherStationConfigurationVM
    {
        public WeatherStationConfigurationVM()
        {
            ApiKey = new PropertyVM<string>(string.Empty);
            Latitude = new PropertyVM<string>(string.Empty);
            Longitude = new PropertyVM<string>(string.Empty);
        }

        public PropertyVM<string> Longitude { get; set; }

        public PropertyVM<string> Latitude { get; }

        public PropertyVM<string> ApiKey { get; }

        public ICommand RefreshCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand TestCommand { get; }
    }
}
