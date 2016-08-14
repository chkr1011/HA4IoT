using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.PersonalAgent;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.OutdoorHumidity;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Services.Weather;
using HA4IoT.Controller.Main.Rooms;
using HA4IoT.ExternalServices.OpenWeatherMap;
using HA4IoT.ExternalServices.TelegramBot;
using HA4IoT.ExternalServices.Twitter;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Hardware.Pi2;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Hardware.RemoteSwitch.Codes;
using HA4IoT.PersonalAgent;
using HA4IoT.Services.ControllerSlave;
using HA4IoT.Services.Environment;

namespace HA4IoT.Controller.Main
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var controller = new Controller { Initializer = new Initializer() };
            controller.RunAsync(taskInstance);
        }

        private class Initializer : IInitializer
        {
            public void RegisterServices(IContainerService containerService)
            {
                RegisterI2CHardwareBridgeService(containerService);
                RegisterOpenWeatherMapService(containerService);
                RegisterControllerSlaveService(containerService);
                RegisterTwitterClientService(containerService);
                RegisterTelegramBotService(containerService);
            }
            
            public Task Initialize(IContainerService containerService)
            {
                var ccToolsBoardService = containerService.GetInstance<CCToolsBoardService>();
                var pi2GpioService = containerService.GetInstance<Pi2GpioService>();
                var synonymService = containerService.GetInstance<SynonymService>();
              
                synonymService.TryLoadPersistedSynonyms();

                ccToolsBoardService.CreateHSPE16InputOnly(InstalledDevice.Input0, new I2CSlaveAddress(42));
                ccToolsBoardService.CreateHSPE16InputOnly(InstalledDevice.Input1, new I2CSlaveAddress(43));
                ccToolsBoardService.CreateHSPE16InputOnly(InstalledDevice.Input2, new I2CSlaveAddress(47));
                ccToolsBoardService.CreateHSPE16InputOnly(InstalledDevice.Input3, new I2CSlaveAddress(45));
                ccToolsBoardService.CreateHSPE16InputOnly(InstalledDevice.Input4, new I2CSlaveAddress(46));
                ccToolsBoardService.CreateHSPE16InputOnly(InstalledDevice.Input5, new I2CSlaveAddress(44));

                containerService.GetInstance<BedroomConfiguration>().Setup();
                containerService.GetInstance<OfficeConfiguration>().Setup();
                containerService.GetInstance<UpperBathroomConfiguration>().Setup();
                containerService.GetInstance<ReadingRoomConfiguration>().Setup();
                containerService.GetInstance<ChildrensRoomRoomConfiguration>().Setup();
                containerService.GetInstance<KitchenConfiguration>().Setup();
                containerService.GetInstance<FloorConfiguration>().Setup();
                containerService.GetInstance<LowerBathroomConfiguration>().Setup();
                containerService.GetInstance<StoreroomConfiguration>().Setup();
                containerService.GetInstance<LivingRoomConfiguration>().Setup();

                synonymService.RegisterDefaultComponentStateSynonyms();

                InitializeAzureCloudApiEndpoint();

                var ioBoardsInterruptMonitor = new InterruptMonitor(pi2GpioService.GetInput(4));
                ioBoardsInterruptMonitor.InterruptDetected += (s, e) => ccToolsBoardService.PollInputBoardStates();
                ioBoardsInterruptMonitor.Start();

                return Task.FromResult(0);
            }

            private void RegisterOpenWeatherMapService(IContainerService containerService)
            {
                var dateTimeService = containerService.GetInstance<IDateTimeService>();
                var openWeatherMapService = containerService.GetInstance<OpenWeatherMapService>();

                containerService.RegisterSingleton(() => openWeatherMapService);
                containerService.RegisterSingleton<IOutdoorTemperatureService>(() => new OutdoorTemperatureService(openWeatherMapService, dateTimeService));
                containerService.RegisterSingleton<IOutdoorHumidityService>(() => new OutdootHumidityService(openWeatherMapService, dateTimeService));
                containerService.RegisterSingleton<IDaylightService>(() => new DaylightService(openWeatherMapService, dateTimeService));
                containerService.RegisterSingleton<IWeatherService>(() => new WeatherService(openWeatherMapService, dateTimeService));
            }

            private void RegisterControllerSlaveService(IContainerService containerService)
            {
                var controllerSlaveServiceOptions = new ControllerSlaveServiceOptions
                {
                    MasterControllerAddress = "127.0.0.1"
                };

                containerService.RegisterSingleton(() => controllerSlaveServiceOptions);
                containerService.RegisterSingleton<ControllerSlaveService>();
            }

            private void RegisterTelegramBotService(IContainerService containerService)
            {
                TelegramBotService telegramBotService;
                if (!TelegramBotServiceFactory.TryCreateFromDefaultConfigurationFile(out telegramBotService))
                {
                    return;
                }

                Log.WarningLogged += (s, e) =>
                {
                    telegramBotService.EnqueueMessageForAdministrators($"{Emoji.WarningSign} {e.Message}\r\n{e.Exception}", TelegramMessageFormat.PlainText);
                };

                Log.ErrorLogged += (s, e) =>
                {
                    if (e.Message.StartsWith("Sending Telegram message failed"))
                    {
                        // Prevent recursive send of sending failures.
                        return;
                    }

                    telegramBotService.EnqueueMessageForAdministrators($"{Emoji.HeavyExclamationMark} {e.Message}\r\n{e.Exception}", TelegramMessageFormat.PlainText);
                };

                telegramBotService.EnqueueMessageForAdministrators($"{Emoji.Bell} Das System ist gestartet.");

                containerService.GetInstance<PersonalAgentToTelegramBotDispatcher>().ExposeToTelegramBot(telegramBotService);
                containerService.RegisterSingleton(() => telegramBotService);
            }

            private void RegisterTwitterClientService(IContainerService containerService)
            {
                TwitterClientService twitterClientService;
                if (!TwitterClientServiceFactory.TryCreateFromDefaultConfigurationFile(out twitterClientService))
                {
                    return;
                }

                containerService.RegisterSingleton(() => twitterClientService);
            }

            private void RegisterI2CHardwareBridgeService(IContainerService containerService)
            {
                const int LDP433MhzSenderPin = 10;

                var deviceService = containerService.GetInstance<IDeviceService>();
                var i2CBusService = containerService.GetInstance<II2CBusService>();
                var schedulerService = containerService.GetInstance<ISchedulerService>();
                var apiService = containerService.GetInstance<IApiService>();

                var i2CHardwareBridge = new I2CHardwareBridge(new I2CSlaveAddress(50), i2CBusService, schedulerService);
                deviceService.AddDevice(i2CHardwareBridge);
                
                var brennenstuhl = new BrennenstuhlCodeSequenceProvider();
                var ldp433MHzSender = new LPD433MHzSignalSender(i2CHardwareBridge, LDP433MhzSenderPin, apiService);

                var remoteSwitchService = new RemoteSocketService(ldp433MHzSender, schedulerService)
                    .WithRemoteSocket(0, brennenstuhl.GetSequencePair(BrennenstuhlSystemCode.AllOn, BrennenstuhlUnitCode.A));

                containerService.RegisterSingleton(() => remoteSwitchService);
            }
        }
    }
}