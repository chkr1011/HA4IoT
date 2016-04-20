using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HA4IoT.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Sensors;
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
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.PersonalAgent;
using HA4IoT.Contracts.Services.WeatherService;
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
    
            RegisterService(new SynonymService(ApiController));
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

            ////var localCsvFileWriter = new CsvHistory(Logger, ApiController);
            ////localCsvFileWriter.ConnectActuators(this);
            ////localCsvFileWriter.ExposeToApi(ApiController);

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
            var messageContextFactory = new MessageContextFactory(GetService<SynonymService>());
            MessageContext messageContext = messageContextFactory.Create(e.Message);

            if (messageContext.GetPatternMatch("Hi").Success)
            {
                await e.SendResponse($"{Emoji.VictoryHand} Hi, was kann ich für Dich tun?");
                return;
            }

            if (messageContext.GetPatternMatch("Danke").Success)
            {
                await e.SendResponse($"{Emoji.Wink} Gerne.");
                return;
            }

            if (messageContext.GetPatternMatch("Wetter").Success)
            {
                var weatherService = GetService<IWeatherService>();

                var response = new StringBuilder();
                response.AppendLine($"{Emoji.BarChart} Das Wetter ist aktuell:");
                response.AppendLine($"Temperatur: {weatherService.GetTemperature()}°C");
                response.AppendLine($"Luftfeuchtigkeit: {weatherService.GetHumidity()}%");
                response.AppendLine($"Situation: {weatherService.GetSituation()}");
                
                await e.SendResponse(response.ToString());

                return;
            }

            if (messageContext.IdentifiedComponentIds.Count > 1)
            {
                await e.SendResponse("Bitte nicht mehrere Komponenten auf einmal.");
                return;
            }

            if (messageContext.IdentifiedComponentIds.Count == 1)
            {
                var component = GetComponent<IComponent>(messageContext.IdentifiedComponentIds.First());

                IActuator actuator = component as IActuator;
                if (actuator != null)
                {
                    if (messageContext.IdentifiedComponentStates.Count == 0)
                    {
                        await e.SendResponse($"{Emoji.Confused} Was soll ich damit machen?");
                        return;
                    }
                    else if (messageContext.IdentifiedComponentStates.Count > 1)
                    {
                        await e.SendResponse($"{Emoji.Confused} Das was du willst ist nicht eindeutig.");
                        return;
                    }
                    else
                    {
                        if (!actuator.GetSupportsState(messageContext.IdentifiedComponentStates.First()))
                        {
                            await e.SendResponse($"{Emoji.Confused} Das wird nicht funktionieren.");
                            return;
                        }
                        else
                        {
                            actuator.SetState(messageContext.IdentifiedComponentStates.First());
                            await e.SendResponse($"{Emoji.WhiteCheckMark} Habe ich erledigt.");
                            return;
                        }
                    }
                }

                ISensor sensor = component as ISensor;
                if (sensor != null)
                {
                    await e.SendResponse($"Der sensor hat momentan den folgenden Zustand: {sensor.GetState()}");
                    return;
                }
            }

            if (messageContext.GetPatternMatch("Fenster.*geschlossen").Success)
            {
                var allWindows = GetComponents<IWindow>();
                var openWindows = allWindows.Where(w => w.Casements.Any(c => c.GetState() != CasementStateId.Closed)).ToList();

                string response;
                if (!openWindows.Any())
                {
                    response = "Alle Fenster sind geschlossen.";
                }
                else
                {
                    response = "Nein! Die folgenden Fenster sind noch (ganz oder auf Kipp) geöffnet:\r\n";
                    response += string.Join(Environment.NewLine, openWindows.Select(w => "- " + w.Id));
                }

                await e.TelegramBot.TrySendMessageAsync(e.Message.CreateResponse(response));
                return;
            }
            
            await e.TelegramBot.TrySendMessageAsync(e.Message.CreateResponse($"{Emoji.Confused} Das habe ich nicht verstanden. Bitte stelle Deine Anfrage präziser."));
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
