using System;
using System.Threading.Tasks;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Services.ExternalServices;
using HA4IoT.Contracts.Services.ExternalServices.Twitter;

namespace HA4IoT.ExternalServices.Twitter
{
    public class TweetAction : IAction
    {
        private readonly Func<string> _messageProvider;
        private readonly ITwitterClientService _twitterService;

        public TweetAction(Func<string> messageProvider, ITwitterClientService twitterService)
        {
            if (messageProvider == null) throw new ArgumentNullException(nameof(messageProvider));
            if (twitterService == null) throw new ArgumentNullException(nameof(twitterService));

            _messageProvider = messageProvider;
            _twitterService = twitterService;
        }

        public TweetAction(string message, ITwitterClientService twitterService)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (twitterService == null) throw new ArgumentNullException(nameof(twitterService));

            _messageProvider = () => message;
            _twitterService = twitterService;
        }

        public void Execute()
        {
            Task.Run(async () => await _twitterService.Tweet(_messageProvider()));
        }
    }
}
