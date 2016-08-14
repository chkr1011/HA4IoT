using System;
using System.IO;
using Windows.Data.Json;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.ExternalServices.TelegramBot
{
    public static class TelegramBotServiceOptionsFactory
    {
        public static bool TryCreateFromDefaultConfigurationFile(out TelegramBotServiceOptions options)
        {
            string filename = StoragePath.WithFilename("TelegramBotConfiguration.json");
            return TryCreateFromConfigurationFile(filename, out options);
        }

        public static bool TryCreateFromConfigurationFile(string filename, out TelegramBotServiceOptions options)
        {
            options = null;

            try
            {
                if (!File.Exists(filename))
                {
                    Log.Verbose($"Telegram bot configuration file '{filename}' not found.");
                    return false;
                }

                string fileContent = File.ReadAllText(filename);
                JsonObject json = JsonObject.Parse(fileContent);

                options = new TelegramBotServiceOptions();
                options.AuthenticationToken = json.GetNamedString("AuthenticationToken");

                foreach (var administratorItem in json.GetNamedArray("Administrators", new JsonArray()))
                {
                    options.Administrators.Add((int)administratorItem.GetNumber());
                }

                foreach (var chatWhitelistItem in json.GetNamedArray("ChatWhitelist", new JsonArray()))
                {
                    options.ChatWhitelist.Add((int)chatWhitelistItem.GetNumber());
                }

                if (json.GetNamedBoolean("AllowAllClients", false))
                {
                    options.AllowAllClients = true;
                }
                
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
