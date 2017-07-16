using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.ExternalServices.TelegramBot;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.PersonalAgent;
using HA4IoT.Contracts.Scripting;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Settings;
using Newtonsoft.Json.Linq;
using HttpClient = System.Net.Http.HttpClient;

namespace HA4IoT.ExternalServices.TelegramBot
{
    public class TelegramBotService : ServiceBase, ITelegramBotService
    {
        private const string BaseUri = "https://api.telegram.org/bot";

        private readonly BlockingCollection<TelegramOutboundMessage> _pendingMessages = new BlockingCollection<TelegramOutboundMessage>();
        private readonly IPersonalAgentService _personalAgentService;
        private readonly ILogger _log;

        private int _latestUpdateId;
        private bool _isConnected;

        public TelegramBotService(
            ISettingsService settingsService, 
            IPersonalAgentService personalAgentService,
            ISystemInformationService systemInformationService,
            ILogService logService,
            IScriptingService scriptingService)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));

            _personalAgentService = personalAgentService ?? throw new ArgumentNullException(nameof(personalAgentService));

            _log = logService.CreatePublisher(nameof(TelegramBotService));

            settingsService.CreateSettingsMonitor<TelegramBotServiceSettings>(s => Settings = s.NewSettings);
            systemInformationService.Set("TelegramBotService/IsConnected", () => _isConnected);

            scriptingService.RegisterScriptProxy(s => new TelegramBotScriptProxy(this));
        }

        public TelegramBotServiceSettings Settings { get; private set; }

        public override void Startup()
        {
            Task.Run(ProcessPendingMessagesAsync);
            Task.Run(WaitForUpdates);
        }

        public void EnqueueMessage(TelegramOutboundMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (!Settings.IsEnabled)
            {
                return;
            }

            _pendingMessages.Add(message);
        }

        public void EnqueueMessageForAdministrators(string text, TelegramMessageFormat format = TelegramMessageFormat.HTML)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            if (!Settings.IsEnabled)
            {
                return;
            }

            foreach (var chatId in Settings.Administrators)
            {
                EnqueueMessage(new TelegramOutboundMessage(chatId, text, format));
            }
        }

        private async Task SendMessageAsync(TelegramOutboundMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            using (var httpClient = new HttpClient())
            {
                var uri = $"{BaseUri}{Settings.AuthenticationToken}/sendMessage";
                var body = ConvertOutboundMessageToJsonMessage(message);
                var response = await httpClient.PostAsync(uri, body);

                if (!response.IsSuccessStatusCode)
                {
                    _log.Warning($"Sending Telegram message failed (Message='${message.Text}' StatusCode={response.StatusCode}).");
                }
                else
                {
                    _log.Info($"Sent Telegram message '{message.Text}' to chat {message.ChatId}.");
                }
            }
        }

        private async Task ProcessPendingMessagesAsync()
        {
            while (true)
            {
                try
                {
                    await SendMessageAsync(_pendingMessages.Take());
                }
                catch (Exception exception)
                {
                    _log.Error(exception, "Error while processing pending messages.");
                }
            }
        }

        private async Task WaitForUpdates()
        {
            while (true)
            {
                try
                {
                    await WaitForNextUpdates();
                }
                catch (TaskCanceledException)
                {                    
                }
                catch (Exception exception)
                {
                    _log.Warning(exception, "Error while waiting for next updates.");
                }
            }
        }

        private async Task WaitForNextUpdates()
        {
            if (!Settings.IsEnabled)
            {
                await Task.Delay(TimeSpan.FromMinutes(1));
                return;
            }

            using (var httpClient = new HttpClient())
            {
                var uri = $"{BaseUri}{Settings.AuthenticationToken}/getUpdates?timeout=60&offset={_latestUpdateId + 1}";
                var response = await httpClient.GetAsync(uri);

                _isConnected = response.IsSuccessStatusCode;

                if (!response.IsSuccessStatusCode)
                {
                    _log.Warning($"Failed to fetch new updates (StatusCode={response.StatusCode}).");
                    return;
                }

                var body = await response.Content.ReadAsStringAsync();
                ProcessUpdates(body);
            }
        }

        private void ProcessUpdates(string body)
        {
            JObject response;
            try
            {
                response = JObject.Parse(body);
            }
            catch (Exception exception)
            {
                _log.Warning(exception, "Unable to process updates.");
                return;
            }
            
            if (!(bool)response["ok"])
            {
                return;
            }

            foreach (var updateItem in response["result"].ToObject<JArray>())
            {
                var update = updateItem.ToObject<JObject>();

                _latestUpdateId = (int)update["update_id"];
                ProcessMessage((JObject)update["message"]);
            }
        }

        private void ProcessMessage(JObject message)
        {
            var inboundMessage = ConvertJsonMessageToInboundMessage(message);

            if (!Settings.AllowAllClients && !Settings.ChatWhitelist.Contains(inboundMessage.ChatId))
            {
                EnqueueMessage(inboundMessage.CreateResponse("Not authorized!"));

                EnqueueMessageForAdministrators(
                    $"{Emoji.WarningSign} A none whitelisted client ({inboundMessage.ChatId}) has sent a message: '{inboundMessage.Text}'");
            }
            else
            {
                var answer = _personalAgentService.ProcessTextMessage(inboundMessage.Text);
                var response = inboundMessage.CreateResponse(answer);
                EnqueueMessage(response);
            }
        }

        private TelegramInboundMessage ConvertJsonMessageToInboundMessage(JObject message)
        {
            var text = (string)message["text"];
            var timestamp = UnixTimeStampToDateTime((long)message["date"]);

            var chat = (JObject)message["chat"];
            var chatId = (int)chat["id"];

            return new TelegramInboundMessage(timestamp, chatId, text);
        }

        private StringContent ConvertOutboundMessageToJsonMessage(TelegramOutboundMessage message)
        {
            if (message.Text.Length > 4096)
            {
                throw new InvalidOperationException("The Telegram outbound message is too long.");
            }

            var json = new JObject
            {
                ["chat_id"] = message.ChatId,
                ["text"] = message.Text
            };

            if (message.Format == TelegramMessageFormat.HTML)
            {
                json["parse_mode"] = "HTML";
            }

            return new StringContent(json.ToString(), Encoding.UTF8, "application/json");
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var buffer = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return buffer.AddSeconds(unixTimeStamp).ToLocalTime();
        }
    }
}
