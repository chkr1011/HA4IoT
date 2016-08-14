using System.Threading.Tasks;
using HA4IoT.Configuration;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Services.Weather;
using HA4IoT.Core;
using HA4IoT.ExternalServices.OpenWeatherMap;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Hardware.Pi2;
using HA4IoT.Services.Environment;

namespace HA4IoT.Controller.Default
{
    internal class Controller : ControllerBase
    {
        private const int LedGpio = 22;

        public Controller()
            : base(LedGpio)
        {
        }

        protected override async Task ConfigureAsync()
        {
            var pi2PortController = new Pi2GpioService();
            var ccToolsBoardController = new CCToolsBoardService(this, GetDevice<II2CBusService>());

            AddDevice(pi2PortController);
            AddDevice(ccToolsBoardController);

            var openWeatherMapService = new OpenWeatherMapService(
                ServiceLocator.GetService<IDateTimeService>(),
                ServiceLocator.GetService<ISchedulerService>(),
                ServiceLocator.GetService<ISystemInformationService>());

            ServiceLocator.RegisterService(typeof(IOutdoorTemperatureService), new OutdoorTemperatureService(openWeatherMapService, ServiceLocator.GetService<IDateTimeService>()));
            ServiceLocator.RegisterService(typeof(IOutdoorHumidityService), new OutdootHumidityService(openWeatherMapService, ServiceLocator.GetService<IDateTimeService>()));
            ServiceLocator.RegisterService(typeof(IDaylightService), new DaylightService(openWeatherMapService, ServiceLocator.GetService<IDateTimeService>()));
            ServiceLocator.RegisterService(typeof(IWeatherService), new WeatherService(openWeatherMapService, ServiceLocator.GetService<IDateTimeService>()));
            ServiceLocator.RegisterService(typeof(OpenWeatherMapService), openWeatherMapService);

            var configurationParser = new ConfigurationParser(this);
            configurationParser.RegisterConfigurationExtender(new DefaultConfigurationExtender(configurationParser, this));
            configurationParser.RegisterConfigurationExtender(new CCToolsConfigurationExtender(configurationParser, this));
            configurationParser.RegisterConfigurationExtender(new I2CHardwareBridgeConfigurationExtender(configurationParser, this));
            configurationParser.ParseConfiguration();

            InitializeAzureCloudApiEndpoint();

            var ioBoardsInterruptMonitor = new InterruptMonitor(pi2PortController.GetInput(4));
            ioBoardsInterruptMonitor.InterruptDetected += (s, e) => ccToolsBoardController.PollInputBoardStates();
            ioBoardsInterruptMonitor.Start();

            await base.ConfigureAsync();
        }
    }
}
