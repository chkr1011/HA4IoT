using System;
using System.IO;
using Windows.Data.Json;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.ExternalServices.TelegramBot
{
    public static class TelegramBotServiceFactory
    {
        public static bool TryCreateFromDefaultConfigurationFile(out TelegramBotService telegramBotService)
        {
            string filename = StoragePath.WithFilename("TelegramBotConfiguration.json");
            return TryCreateFromConfigurationFile(filename, out telegramBotService);
        }

        public static bool TryCreateFromConfigurationFile(string filename, out TelegramBotService telegramBotService)
        {
            telegramBotService = null;

            try
            {
                if (!File.Exists(filename))
                {
                    Log.Verbose($"Telegram bot configuration file '{filename}' not found.");
                    return false;
                }

                string fileContent = File.ReadAllText(filename);
                JsonObject json = JsonObject.Parse(fileContent);

                telegramBotService = new TelegramBotService();
                telegramBotService.AuthenticationToken = json.GetNamedString("AuthenticationToken");

                foreach (var administratorItem in json.GetNamedArray("Administrators", new JsonArray()))
                {
                    telegramBotService.Administrators.Add((int)administratorItem.GetNumber());
                }

                foreach (var chatWhitelistItem in json.GetNamedArray("ChatWhitelist", new JsonArray()))
                {
                    telegramBotService.ChatWhitelist.Add((int)chatWhitelistItem.GetNumber());
                }

                if (json.GetNamedBoolean("AllowAllClients", false))
                {
                    telegramBotService.AllowAllClients = true;
                }

                telegramBotService.Enable();

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
