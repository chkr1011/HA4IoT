using System;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;

namespace HA4IoT.ExternalServices.Twitter
{
    public static class TwitterClientServiceFactory
    {
        public static bool TryCreateFromConfigurationFile(string filename, out TwitterClientService twitterService)
        {
            if (!File.Exists(filename))
            {
                twitterService = null;
                return false;
            }

            try
            {
                string fileContent = File.ReadAllText(filename);
                JsonObject configuration;
                if (!JsonObject.TryParse(fileContent, out configuration))
                {
                    twitterService = null;
                    return false;
                }

                twitterService = new TwitterClientService();
                twitterService.AccessToken = configuration.GetNamedString("AccessToken");
                twitterService.AccessTokenSecret = configuration.GetNamedString("AccessTokenSecret");
                twitterService.CosumerSecret = configuration.GetNamedString("ConsumerSecret");
                twitterService.ConsumerKey = configuration.GetNamedString("ConsumerKey");

                return true;
            }
            catch (Exception)
            {
                twitterService = null;
                return false;
            }
        }

        public static bool TryCreateFromDefaultConfigurationFile(out TwitterClientService twitterService)
        {
            string filename = Path.Combine(ApplicationData.Current.LocalFolder.Path, "TwitterClientConfiguration.json");
            return TryCreateFromConfigurationFile(filename, out twitterService);
        }
    }
}
