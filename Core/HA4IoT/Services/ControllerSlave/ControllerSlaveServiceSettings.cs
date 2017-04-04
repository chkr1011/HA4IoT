using HA4IoT.Contracts.Services.Settings;

namespace HA4IoT.Services.ControllerSlave
{
    public class ControllerSlaveServiceSettings
    {
        public bool IsEnabled { get; set; }

        public string MasterAddress { get; set; }

        public bool UseTemperature { get; set; }

        public bool UseHumidity { get; set; }

        public bool UseSunriseSunset { get; set; }

        public bool UseWeather { get; set; }
    }
}
