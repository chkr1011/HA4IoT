using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.PersonalAgent;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.ExternalServices.TelegramBot;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Networking.Json;
using HA4IoT.PersonalAgent;
using HttpClient = System.Net.Http.HttpClient;

namespace HA4IoT.ExternalServices.TelegramBot
{
    public class TelegramBotService : ServiceBase, ITelegramBotService
    {
        private const string BaseUri = "https://api.telegram.org/bot";

        private readonly BlockingCollection<TelegramOutboundMessage> _pendingMessages = new BlockingCollection<TelegramOutboundMessage>();
        private readonly IPersonalAgentService _personalAgentService;
        private readonly TelegramBotServiceSettings _settings;

        private int _latestUpdateId;

        public TelegramBotService(ISettingsService settingsService, IPersonalAgentService personalAgentService)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (personalAgentService == null) throw new ArgumentNullException(nameof(personalAgentService));

            _settings = settingsService.GetSettings<TelegramBotServiceSettings>();

            _personalAgentService = personalAgentService;

            Log.WarningLogged += (s, e) =>
            {
                EnqueueMessageForAdministrators($"{Emoji.WarningSign} {e.Message}\r\n{e.Exception}", TelegramMessageFormat.PlainText);
            };

            Log.ErrorLogged += (s, e) =>
            {
                if (e.Message.StartsWith("Sending Telegram message failed"))
                {
                    // Prevent recursive send of sending failures.
                    return;
                }

                EnqueueMessageForAdministrators($"{Emoji.HeavyExclamationMark} {e.Message}\r\n{e.Exception}", TelegramMessageFormat.PlainText);
            };
        }

        public override void Startup()
        {
            if (!_settings.IsEnabled)
            {
                Log.Verbose("Telegram Bot Service is disabled.");
                return;
            }

            Task.Factory.StartNew(
                async () => await ProcessPendingMessagesAsync(),
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            Task.Factory.StartNew(
                async () => await WaitForUpdates(),
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            EnqueueMessageForAdministrators($"{Emoji.Bell} Das System ist gestartet.");
        }

        public void EnqueueMessage(TelegramOutboundMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (!_settings.IsEnabled)
            {
                return;
            }

            _pendingMessages.Add(message);
        }

        public void EnqueueMessageForAdministrators(string text, TelegramMessageFormat format = TelegramMessageFormat.HTML)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            if (!_settings.IsEnabled)
            {
                return;
            }

            foreach (var chatId in _settings.Administrators)
            {
                EnqueueMessage(new TelegramOutboundMessage(chatId, text, format));
            }
        }

        private async Task SendMessageAsync(TelegramOutboundMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            using (var httpClient = new HttpClient())
            {
                string uri = $"{BaseUri}{_settings.AuthenticationToken}/sendMessage";
                StringContent body = ConvertOutboundMessageToJsonMessage(message);
                HttpResponseMessage response = await httpClient.PostAsync(uri, body);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning(
                        $"Sending Telegram message failed (Message='${message.Text}' StatusCode={response.StatusCode}).");
                }
                else
                {
                    Log.Info($"Sent Telegram message '{message.Text}' to chat {message.ChatId}.");
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
                    Log.Error(exception, "Error while processing pending Telegram messages.");
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
                    Log.Warning(exception, "Error while waiting for next Telegram updates.");
                }
            }
        }

        private async Task WaitForNextUpdates()
        {
            using (var httpClient = new HttpClient())
            {
                var uri = $"{BaseUri}{_settings.AuthenticationToken}/getUpdates?timeout=60&offset={_latestUpdateId + 1}";
                var response = await httpClient.GetAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Failed to fetch new updates of TelegramBot (StatusCode={response.StatusCode}).");
                }

                string body = await response.Content.ReadAsStringAsync();
                ProcessUpdates(body);
            }
        }

        private void ProcessUpdates(string body)
        {
            JsonObject response;
            if (!JsonObject.TryParse(body, out response))
            {
                return;
            }

            if (!response.GetNamedBoolean("ok", false))
            {
                return;
            }

            foreach (var updateItem in response.GetNamedArray("result"))
            {
                JsonObject update = updateItem.GetObject();

                _latestUpdateId = (int)update.GetNamedNumber("update_id");
                ProcessMessage(update.GetNamedObject("message"));
            }
        }

        private void ProcessMessage(JsonObject message)
        {
            var inboundMessage = ConvertJsonMessageToInboundMessage(message);

            if (!_settings.AllowAllClients && !_settings.ChatWhitelist.Contains(inboundMessage.ChatId))
            {
                EnqueueMessage(inboundMessage.CreateResponse("Not authorized!"));

                EnqueueMessageForAdministrators(
                    $"{Emoji.WarningSign} A none whitelisted client ({inboundMessage.ChatId}) has sent a message: '{inboundMessage.Text}'");
            }
            else
            {
                var answer = _personalAgentService.ProcessMessage(inboundMessage);
                var response = inboundMessage.CreateResponse(answer);
                EnqueueMessage(response);
            }
        }

        private TelegramInboundMessage ConvertJsonMessageToInboundMessage(JsonObject message)
        {
            var text = message.GetNamedString("text");
            var timestamp = UnixTimeStampToDateTime(message.GetNamedNumber("date"));

            var chat = message.GetNamedObject("chat");
            var chatId = (int)chat.GetNamedNumber("id");

            return new TelegramInboundMessage(timestamp, chatId, text);
        }

        private StringContent ConvertOutboundMessageToJsonMessage(TelegramOutboundMessage message)
        {
            if (message.Text.Length > 4096)
            {
                throw new InvalidOperationException("The Telegram outbound message is too long.");
            }

            var json = new JsonObject();
            json.SetValue("chat_id", message.ChatId);
            json.SetValue("text", message.Text);

            if (message.Format == TelegramMessageFormat.HTML)
            {
                json.SetValue("parse_mode", "HTML");
            }

            return new StringContent(json.Stringify(), Encoding.UTF8, "application/json");
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var buffer = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return buffer.AddSeconds(unixTimeStamp).ToLocalTime();
        }
    }
}
