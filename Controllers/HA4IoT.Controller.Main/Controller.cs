using System.Threading.Tasks;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Controller.Main.Rooms;
using HA4IoT.Core;
using HA4IoT.ExternalServices.OpenWeatherMap;
using HA4IoT.ExternalServices.TelegramBot;
using HA4IoT.ExternalServices.Twitter;
using HA4IoT.Hardware;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Hardware.Pi2;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Hardware.RemoteSwitch.Codes;
using HA4IoT.Contracts.PersonalAgent;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.PersonalAgent;

namespace HA4IoT.Controller.Main
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
            AddDevice(new BuiltInI2CBus());
            
            var ccToolsBoardController = new CCToolsBoardController(this, GetDevice<II2CBus>());

            AddDevice(new Pi2PortController());
            AddDevice(ccToolsBoardController);
            AddDevice(new I2CHardwareBridge(new I2CSlaveAddress(50), GetDevice<II2CBus>(), ServiceLocator.GetService<ISchedulerService>()));
            AddDevice(SetupRemoteSwitchController());

            ServiceLocator.RegisterService(typeof(SynonymService), new SynonymService());
            ServiceLocator.RegisterService(typeof(OpenWeatherMapService), 
                new OpenWeatherMapService(
                    ApiController,
                    ServiceLocator.GetService<IDateTimeService>(),
                    ServiceLocator.GetService<ISystemInformationService>()));

            SetupTelegramBot();
            SetupTwitterClient();

            ServiceLocator.GetService<SynonymService>().TryLoadPersistedSynonyms();

            ccToolsBoardController.CreateHSPE16InputOnly(InstalledDevice.Input0, new I2CSlaveAddress(42));
            ccToolsBoardController.CreateHSPE16InputOnly(InstalledDevice.Input1, new I2CSlaveAddress(43));
            ccToolsBoardController.CreateHSPE16InputOnly(InstalledDevice.Input2, new I2CSlaveAddress(47));
            ccToolsBoardController.CreateHSPE16InputOnly(InstalledDevice.Input3, new I2CSlaveAddress(45));
            ccToolsBoardController.CreateHSPE16InputOnly(InstalledDevice.Input4, new I2CSlaveAddress(46));
            ccToolsBoardController.CreateHSPE16InputOnly(InstalledDevice.Input5, new I2CSlaveAddress(44));

            new BedroomConfiguration(this).Setup();
            new OfficeConfiguration(this).Setup();
            new UpperBathroomConfiguration(this).Setup();
            new ReadingRoomConfiguration(this).Setup();
            new ChildrensRoomRoomConfiguration(this).Setup();
            new KitchenConfiguration(this).Setup();
            new FloorConfiguration(this).Setup();
            new LowerBathroomConfiguration(this).Setup();
            new StoreroomConfiguration(this).Setup();
            new LivingRoomConfiguration(this).Setup();

            ServiceLocator.GetService<SynonymService>().RegisterDefaultComponentStateSynonyms(this);

            InitializeAzureCloudApiEndpoint();

            var ioBoardsInterruptMonitor = new InterruptMonitor(GetDevice<Pi2PortController>().GetInput(4));
            ioBoardsInterruptMonitor.InterruptDetected += (s, e) => ccToolsBoardController.PollInputBoardStates();
            ioBoardsInterruptMonitor.Start();

            await base.ConfigureAsync();
        }

        private void SetupTelegramBot()
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
            new PersonalAgentToTelegramBotDispatcher(this).ExposeToTelegramBot(telegramBotService);

            ServiceLocator.RegisterService(typeof(TelegramBotService), telegramBotService);
        }

        private void SetupTwitterClient()
        {
            TwitterService twitterService;
            if (!TwitterServiceFactory.TryCreateFromDefaultConfigurationFile(out twitterService))
            {
                return;
            }

            ServiceLocator.RegisterService(typeof(TwitterService), twitterService);
        }

        private RemoteSocketController SetupRemoteSwitchController()
        {
            const int LDP433MhzSenderPin = 10;

            var i2cHardwareBridge = GetDevice<I2CHardwareBridge>();
            var brennenstuhl = new BrennenstuhlCodeSequenceProvider();
            var ldp433MHzSender = new LPD433MHzSignalSender(i2cHardwareBridge, LDP433MhzSenderPin, ApiController);

            var remoteSwitchController = new RemoteSocketController(ldp433MHzSender, ServiceLocator.GetService<ISchedulerService>())
                .WithRemoteSocket(0, brennenstuhl.GetSequencePair(BrennenstuhlSystemCode.AllOn, BrennenstuhlUnitCode.A));

            return remoteSwitchController;
        }
    }
}
