using System.Threading.Tasks;
using HA4IoT.Configuration;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Core;
using HA4IoT.ExternalServices.OpenWeatherMap;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Hardware.Pi2;

namespace HA4IoT.Controller.Default
{
    internal class Controller : ControllerBase
    {
        private const int LedGpio = 22;

        protected override async Task ConfigureAsync()
        {
            InitializeHealthMonitor(LedGpio);

            AddDevice(new BuiltInI2CBus());

            var pi2PortController = new Pi2PortController();
            var ccToolsBoardController = new CCToolsBoardController(this, GetDevice<II2CBus>());

            AddDevice(pi2PortController);
            AddDevice(ccToolsBoardController);

            RegisterService(new OpenWeatherMapService(ApiController, GetService<IDateTimeService>(), GetService<ISystemInformationService>()));

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
