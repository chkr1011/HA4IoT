using System;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Services.ExternalServices;
using HA4IoT.Contracts.Services.ExternalServices.Twitter;

namespace HA4IoT.ExternalServices.Twitter
{
    public static class TwitterClientServiceExtensions
    {
        public static IAction GetTweetAction(this ITwitterClientService twitterClientService, string message)
        {
            if (twitterClientService == null) throw new ArgumentNullException(nameof(twitterClientService));
            if (message == null) throw new ArgumentNullException(nameof(message));

            return new TweetAction(message, twitterClientService);
        }

        public static IAction GetTweetAction(this ITwitterClientService twitterClientService, Func<string> messageProvider)
        {
            if (twitterClientService == null) throw new ArgumentNullException(nameof(twitterClientService));
            if (messageProvider == null) throw new ArgumentNullException(nameof(messageProvider));

            return new TweetAction(messageProvider, twitterClientService);
        }
    }
}
