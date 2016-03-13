using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Core.Timer;
using HA4IoT.ExternalServices.Twitter;

namespace HA4IoT.Controller.Main.Rooms
{
    internal class CatLitterBoxTwitterSender
    {
        private readonly ILogger _log;
        private const string Suffix = "\r\nTime in litter box: {0}s\r\nNr. this day: {1}\r\n@chkratky";

        private readonly Timeout _timeout = new Timeout(TimeSpan.FromSeconds(30));
        private readonly Random _random = new Random((int)DateTime.Now.Ticks);
        private readonly Stopwatch _timeInLitterBox = new Stopwatch();

        private TimeSpan _effectiveTimeInLitterBox;
        private int _count = 1;
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

        public CatLitterBoxTwitterSender(IHomeAutomationTimer timer, ILogger log)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (log == null) throw new ArgumentNullException(nameof(log));

            _log = log;
            timer.Tick += Tick;
        }

        public CatLitterBoxTwitterSender WithTrigger(IMotionDetector motionDetector)
        {
            if (motionDetector == null) throw new ArgumentNullException(nameof(motionDetector));

            motionDetector.StateChanged += RestartTimer;
            return this;
        }

        private void Tick(object sender, TimerTickEventArgs e)
        {
            if (!_timeout.IsRunning)
            {
                return;
            }

            _timeout.Tick(e);

            if (_timeout.IsElapsed)
            {
                _timeInLitterBox.Stop();

                Task.Run(() => Tweet(_timeInLitterBox.Elapsed));
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

        private async Task Tweet(TimeSpan timeInLitterBox)
        {
            if (GetIsTweetingTooFrequently())
            {
                return;
            }

            if (GetDurationIsTooShort(timeInLitterBox))
            {
                return;
            }

            UpdateCounter();

            string message = GenerateMessage();
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

        private string GenerateMessage()
        {
            string message;
            do
            {
                message = _messages[_random.Next(_messages.Length - 1)];
            } while (message == _previousMessage);

            _previousMessage = message;

            return message + string.Format(Suffix, (int)_effectiveTimeInLitterBox.TotalSeconds, _count);
        }

        private bool GetIsTweetingTooFrequently()
        {
            return _lastTweetTimestamp.HasValue && (DateTime.Now - _lastTweetTimestamp) < TimeSpan.FromMinutes(5);
        }

        private bool GetDurationIsTooShort(TimeSpan timeInLitterBox)
        {
            _effectiveTimeInLitterBox = timeInLitterBox - _timeout.Duration;
            return _effectiveTimeInLitterBox < TimeSpan.FromSeconds(10);
        }

        private void UpdateCounter()
        {
            if (!_lastTweetTimestamp.HasValue)
            {
                return;
            }

            if (_lastTweetTimestamp.Value.Date.Equals(DateTime.Now.Date))
            {
                _count++;
            }
            else
            {
                _count = 1;
            }
        }

        private TwitterClient GetTwitterApiWithCredentials()
        {
            string filename = Path.Combine(ApplicationData.Current.LocalFolder.Path, "TwitterConfiguration.json"); ;
            if (!File.Exists(filename))
            {
                return null;
            }

            var twitterClient = new TwitterClient();

            string fileContent = File.ReadAllText(filename);
            JsonObject configuration = JsonObject.Parse(fileContent);

            twitterClient.AccessToken = configuration.GetNamedString("AccessToken");
            twitterClient.AccessTokenSecret = configuration.GetNamedString("AccessTokenSecret");
            twitterClient.CosumerSecret = configuration.GetNamedString("ConsumerSecret");
            twitterClient.ConsumerKey = configuration.GetNamedString("ConsumerKey");

            return twitterClient;
        }

    }
}
