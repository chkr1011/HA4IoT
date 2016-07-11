using System;
using System.Threading.Tasks;
using HA4IoT.Contracts.Actions;

namespace HA4IoT.ExternalServices.Twitter
{
    public class TweetAction : IAction
    {
        private readonly Func<string> _messageProvider;
        private readonly TwitterService _twitterService;

        public TweetAction(Func<string> messageProvider, TwitterService twitterService)
        {
            if (messageProvider == null) throw new ArgumentNullException(nameof(messageProvider));
            if (twitterService == null) throw new ArgumentNullException(nameof(twitterService));

            _messageProvider = messageProvider;
            _twitterService = twitterService;
        }

        public TweetAction(string message, TwitterService twitterService)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (twitterService == null) throw new ArgumentNullException(nameof(twitterService));

            _messageProvider = () => message;
            _twitterService = twitterService;
        }

        public void Execute()
        {
            Task.Run(() => _twitterService.Tweet(_messageProvider()));
        }
    }
}
