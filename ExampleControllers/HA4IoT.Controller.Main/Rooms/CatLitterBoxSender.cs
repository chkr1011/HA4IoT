using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Notifications;
using HA4IoT.Core.Timer;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class CatLitterBoxTwitterSender
    {
        private readonly INotificationHandler _log;
        private const string Suffix = "\r\nSeconds in litter box: {0}\r\n@chkratky";

        private readonly Timeout _timeout = new Timeout(TimeSpan.FromSeconds(15));
        private readonly Random _random = new Random((int)DateTime.Now.Ticks);
        private readonly Stopwatch _timeInLitterBox = new Stopwatch();

        private DateTime? _lastTweetTimestamp;
        private string _previousMessage = string.Empty;

        // Twitter will not accept the same tweet twice.
        private readonly string[] _messages =
        {
                "I was just using my litter box...",
                "Meow... that was just in time :-)",
                "Used my litter box...",
                "Got some work for you.",
                "Hey! Clean up my litter box.",
                "Had a great time with my litter box.",
                "I just left my litter box.",
                "OMG! I left a big thing for you.",
                "May te poo be with you.",
                "WOW, I think this is my best one.",
                "Hey, this one looks like you :-)"
            };

        public CatLitterBoxTwitterSender(IHomeAutomationTimer timer, INotificationHandler log)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (log == null) throw new ArgumentNullException(nameof(log));

            _log = log;
            timer.Tick += Tick;
        }

        public CatLitterBoxTwitterSender WithTrigger(IMotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            motionDetector.MotionDetected += RestartTimer;
            return this;
        }

        private void Tick(object sender, TimerTickEventArgs e)
        {
            if (!_timeout.IsRunning)
            {
                return;
            }

            _timeout.Tick(e.ElapsedTime);

            if (_timeout.IsElapsed)
            {
                Task.Run(() => Tweet());
            }
        }

        private void RestartTimer(object sender, EventArgs eventArgs)
        {
            if (!_timeInLitterBox.IsRunning)
            {
                _timeInLitterBox.Restart();
            }

            _timeout.Restart();
        }

        private async Task Tweet()
        {
            _timeInLitterBox.Stop();

            bool tweetingTooFrequently = _lastTweetTimestamp.HasValue && (DateTime.Now - _lastTweetTimestamp) < TimeSpan.FromMinutes(5);
            if (tweetingTooFrequently)
            {
                return;
            }

            string message;
            do
            {
                message = _messages[_random.Next(_messages.Length - 1)];
            } while (message == _previousMessage);

            _previousMessage = message;
            message = message + string.Format(Suffix, _timeInLitterBox.Elapsed.TotalSeconds);
            
            _log.Verbose("Trying to tweet '" + message + "'.");

            try
            {
                var twitterApi = GetTwitterApiWithCredentials();
                if (twitterApi == null)
                {
                    _log.Verbose("Twitter API is disabled.");
                    return;
                }

                await twitterApi.Tweet(message);

                _lastTweetTimestamp = DateTime.Now;
                _log.Info("Successfully tweeted: " + message);
            }
            catch (Exception exception)
            {
                _log.Warning("Failed to tweet. " + exception.Message);
            }
        }

        private TwitterApi GetTwitterApiWithCredentials()
        {
            string filename = Path.Combine(ApplicationData.Current.LocalFolder.Path, "TwitterConfiguration.json"); ;
            if (!File.Exists(filename))
            {
                return null;
            }

            var twitterApi = new TwitterApi();

            string fileContent = File.ReadAllText(filename);
            JsonObject configuration = JsonObject.Parse(fileContent);

            twitterApi.AccessToken = configuration.GetNamedString("AccessToken");
            twitterApi.AccessTokenSecret = configuration.GetNamedString("AccessTokenSecret");
            twitterApi.CosumerSecret = configuration.GetNamedString("ConsumerSecret");
            twitterApi.ConsumerKey = configuration.GetNamedString("ConsumerKey");

            return twitterApi;
        }

    }
}
