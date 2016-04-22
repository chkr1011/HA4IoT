using System;
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
using HA4IoT.PersonalAgent;

namespace HA4IoT.Controller.Main
{
    internal class Controller : ControllerBase
    {
        private const int LedGpio = 22;

        protected override void Initialize()
        {
            InitializeHealthMonitor(LedGpio);

            AddDevice(new BuiltInI2CBus());

            var ccToolsBoardController = new CCToolsBoardController(this, GetDevice<II2CBus>());

            AddDevice(new Pi2PortController());
            AddDevice(ccToolsBoardController);
            AddDevice(new I2CHardwareBridge(new I2CSlaveAddress(50), GetDevice<II2CBus>(), Timer));
            AddDevice(SetupRemoteSwitchController());

            var synonymService = new SynonymService();
            try
            {
                synonymService.LoadPersistedSynonyms();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error while loading persisted synonyms.");
            }

            RegisterService(synonymService);
            RegisterService(new OpenWeatherMapWeatherService(Timer, ApiController));

            SetupTwitterClient();
            SetupTelegramBot();

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

            InitializeAzureCloudApiEndpoint();

            var ioBoardsInterruptMonitor = new InterruptMonitor(GetDevice<Pi2PortController>().GetInput(4));
            ioBoardsInterruptMonitor.InterruptDetected += (s, e) => ccToolsBoardController.PollInputBoardStates();
            ioBoardsInterruptMonitor.StartPollingAsync();
        }

        private void SetupTelegramBot()
        {
            TelegramBot telegramBot;
            if (!TelegramBotFactory.TryCreateFromDefaultConfigurationFile(out telegramBot))
            {
                return;
            }

            Log.WarningLogged += (s, e) =>
            {
                Task.Run(
                    async () =>
                        await telegramBot.TrySendMessageToAdministratorsAsync($"{Emoji.WarningSign} {e.Message}\r\n{e.Exception}"));
            };

            Log.ErrorLogged += (s, e) =>
            {
                Task.Run(
                    async () =>
                        await telegramBot.TrySendMessageToAdministratorsAsync($"{Emoji.HeavyExclamationMark} {e.Message}\r\n{e.Exception}"));
            };

            Task.Run(async () => await telegramBot.TrySendMessageToAdministratorsAsync($"{Emoji.Bell} Das System ist gestartet."));
            telegramBot.MessageReceived += HandleTelegramBotMessage;

            RegisterService(telegramBot);
        }

        private void SetupTwitterClient()
        {
            TwitterClient twitterClient;
            if (TwitterClientFactory.TryCreateFromDefaultConfigurationFile(out twitterClient))
            {
                RegisterService(twitterClient);
            }
        }

        private async void HandleTelegramBotMessage(object sender, TelegramBotMessageReceivedEventArgs e)
        {
            var messageProcessor = new PersonalAgentMessageProcessor(this);
            messageProcessor.ProcessMessage(e.Message);

            await e.SendResponse(messageProcessor.Answer);
        }

        private RemoteSocketController SetupRemoteSwitchController()
        {
            const int LDP433MhzSenderPin = 10;

            var i2cHardwareBridge = GetDevice<I2CHardwareBridge>();
            var brennenstuhl = new BrennenstuhlCodeSequenceProvider();
            var ldp433MHzSender = new LPD433MHzSignalSender(i2cHardwareBridge, LDP433MhzSenderPin, ApiController);

            var remoteSwitchController = new RemoteSocketController(ldp433MHzSender, Timer)
                .WithRemoteSocket(0, brennenstuhl.GetSequencePair(BrennenstuhlSystemCode.AllOn, BrennenstuhlUnitCode.A));

            return remoteSwitchController;
        }
    }
}
