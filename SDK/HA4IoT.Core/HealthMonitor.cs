using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Core.Timer;
using HA4IoT.Networking;

namespace HA4IoT.Core
{
    public class HealthMonitor
    {
        private readonly List<int> _durations = new List<int>(100);
        private readonly Timeout _ledTimeout = new Timeout();
        private readonly DateTime _startedDate;
        private readonly IBinaryOutput _statusLed;
        private readonly IHomeAutomationTimer _timer;
        private float? _averageTimerDuration;

        private bool _ledState;
        private float? _maxTimerDuration;

        private float? _minTimerDuration;

        public HealthMonitor(IBinaryOutput statusLed, IHomeAutomationTimer timer, IApiController apiController)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            _statusLed = statusLed;
            _timer = timer;
            _startedDate = _timer.CurrentDateTime;

            if (statusLed != null)
            {
                _ledTimeout.Start(TimeSpan.FromMilliseconds(1));
            }

            timer.Tick += Tick;
            apiController.RouteRequest("health", HandleApiGet);
            apiController.RouteCommand("health/reset", c => ResetStatistics());
        }

        private void HandleApiGet(IApiContext apiContext)
        {
            var status = new JsonObject();
            status.SetNamedNumber("timerMin", _minTimerDuration);
            status.SetNamedValue("timerMax", _maxTimerDuration.ToJsonValue());
            status.SetNamedValue("timerAverage", _averageTimerDuration.ToJsonValue());
            status.SetNamedValue("upTime", (_timer.CurrentDateTime - _startedDate).ToJsonValue());
            status.SetNamedValue("systemTime", _timer.CurrentDateTime.ToJsonValue());

            apiContext.Response = status;
        }

        private void ResetStatistics()
        {
            _minTimerDuration = null;
            _maxTimerDuration = null;
            _averageTimerDuration = null;
        }

        private void Tick(object sender, TimerTickEventArgs e)
        {
            if (_ledTimeout.IsRunning)
            {
                _ledTimeout.Tick(e.ElapsedTime);
                if (_ledTimeout.IsElapsed)
                {
                    ToggleStatusLed();
                }
            }

            _durations.Add((int)e.ElapsedTime.TotalMilliseconds);
            if (_durations.Count == _durations.Capacity)
            {
                _averageTimerDuration = _durations.Sum() / (float)_durations.Count;
                _durations.Clear();

                if (!_maxTimerDuration.HasValue || _averageTimerDuration > _maxTimerDuration.Value)
                {
                    _maxTimerDuration = _averageTimerDuration;
                }

                if (!_minTimerDuration.HasValue || _averageTimerDuration < _minTimerDuration.Value)
                {
                    _minTimerDuration = _averageTimerDuration;
                }
            }
        }

        private void ToggleStatusLed()
        {
            if (_ledState)
            {
                _statusLed.Write(BinaryState.High);
                _ledTimeout.Start(TimeSpan.FromSeconds(5));
            }
            else
            {
                _statusLed.Write(BinaryState.Low);
                _ledTimeout.Start(TimeSpan.FromMilliseconds(200));
            }

            _ledState = !_ledState;
        }
    }
}
