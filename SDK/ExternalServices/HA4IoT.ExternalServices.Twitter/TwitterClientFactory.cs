using System;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;

namespace HA4IoT.ExternalServices.Twitter
{
    public class TwitterClientFactory
    {
        public static bool TryCreateFromConfigurationFile(string filename, out TwitterClient twitterClient)
        {
            if (!File.Exists(filename))
            {
                twitterClient = null;
                return false;
            }

            try
            {
                string fileContent = File.ReadAllText(filename);
                JsonObject configuration;
                if (!JsonObject.TryParse(fileContent, out configuration))
                {
                    twitterClient = null;
                    return false;
                }

                twitterClient = new TwitterClient();
                twitterClient.AccessToken = configuration.GetNamedString("AccessToken");
                twitterClient.AccessTokenSecret = configuration.GetNamedString("AccessTokenSecret");
                twitterClient.CosumerSecret = configuration.GetNamedString("ConsumerSecret");
                twitterClient.ConsumerKey = configuration.GetNamedString("ConsumerKey");

                return true;
            }
            catch (Exception)
            {
                twitterClient = null;
                return false;
            }
        }

        public static bool TryCreateFromDefaultConfigurationFile(out TwitterClient twitterClient)
        {
            string filename = Path.Combine(ApplicationData.Current.LocalFolder.Path, "TwitterClientConfiguration.json");
            return TryCreateFromConfigurationFile(filename, out twitterClient);
        }
    }
}
