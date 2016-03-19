using System;
using System.Threading.Tasks;
using HA4IoT.Contracts.Actions;

namespace HA4IoT.ExternalServices.Twitter
{
    public class TweetAction : IHomeAutomationAction
    {
        private readonly Func<string> _messageProvider;
        private readonly TwitterClient _twitterClient;

        public TweetAction(Func<string> messageProvider, TwitterClient twitterClient)
        {
            if (messageProvider == null) throw new ArgumentNullException(nameof(messageProvider));
            if (twitterClient == null) throw new ArgumentNullException(nameof(twitterClient));

            _messageProvider = messageProvider;
            _twitterClient = twitterClient;
        }

        public TweetAction(string message, TwitterClient twitterClient)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (twitterClient == null) throw new ArgumentNullException(nameof(twitterClient));

            _messageProvider = () => message;
            _twitterClient = twitterClient;
        }

        public void Execute()
        {
            Task.Run(() => _twitterClient.Tweet(_messageProvider()));
        }
    }
}
