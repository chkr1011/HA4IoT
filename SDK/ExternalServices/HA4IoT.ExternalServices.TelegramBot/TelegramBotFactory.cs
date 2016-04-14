using System;
using System.IO;
using Windows.Data.Json;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.ExternalServices.TelegramBot
{
    public static class TelegramBotFactory
    {
        public static bool TryCreateFromDefaultConfigurationFile(out TelegramBot telegramBot)
        {
            string filename = StoragePath.WithFilename("TelegramBotConfiguration.json");
            return TryCreateFromConfigurationFile(filename, out telegramBot);
        }

        public static bool TryCreateFromConfigurationFile(string filename, out TelegramBot telegramBot)
        {
            telegramBot = null;

            try
            {
                if (!File.Exists(filename))
                {
                    Log.Verbose($"Telegram bot configuration file '{filename}' not found.");
                    return false;
                }

                string fileContent = File.ReadAllText(filename);
                JsonObject json = JsonObject.Parse(fileContent);

                telegramBot = new TelegramBot();
                telegramBot.AuthenticationToken = json.GetNamedString("AuthenticationToken");

                foreach (var administratorItem in json.GetNamedArray("Administrators", new JsonArray()))
                {
                    telegramBot.Administrators.Add((int)administratorItem.GetNumber());
                }

                foreach (var chatWhitelistItem in json.GetNamedArray("ChatWhitelist", new JsonArray()))
                {
                    telegramBot.ChatWhitelist.Add((int)chatWhitelistItem.GetNumber());
                }

                telegramBot.StartWaitForMessages();

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Error while initializing Telegram bot.");
                return false;
            }
        }
    }
}
