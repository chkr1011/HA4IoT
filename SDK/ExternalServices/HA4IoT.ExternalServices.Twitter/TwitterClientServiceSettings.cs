using HA4IoT.Contracts.Services.ExternalServices.Twitter;
using HA4IoT.Contracts.Services.Settings;

namespace HA4IoT.ExternalServices.Twitter
{
    [SettingsUri(typeof(ITwitterClientService))]
    public class TwitterClientServiceSettings
    {
        public bool IsEnabled { get; set; }
        public string AccessTokenSecret { get; set; }
        public string AccessToken { get; set; }
        public string ConsumerSecret { get; set; }
        public string ConsumerKey { get; set; }
    }
}
