using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Core.Timer;

namespace HA4IoT.Core
{
    public class HealthService : ServiceBase
    {
        private readonly ISystemInformationService _systemInformationService;

        private readonly List<int> _durations = new List<int>(100);
        private readonly Timeout _ledTimeout = new Timeout();
        private readonly IBinaryOutput _statusLed;
        private float? _averageTimerDuration;

        private bool _ledState;
        private float? _maxTimerDuration;

        private float? _minTimerDuration;

        public HealthService(IBinaryOutput statusLed, IHomeAutomationTimer timer, ISystemInformationService systemInformationService)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));

            _statusLed = statusLed;
            _systemInformationService = systemInformationService;

            if (statusLed != null)
            {
                _ledTimeout.Start(TimeSpan.FromMilliseconds(1));
            }
            
            timer.Tick += Tick;
        }


        public override void HandleApiCommand(IApiContext apiContext)
        {
            ResetStatistics();
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

                _systemInformationService.Set("Health/SystemTime", DateTime.Now);

                if (!_maxTimerDuration.HasValue || _averageTimerDuration > _maxTimerDuration.Value)
                {
                    _maxTimerDuration = _averageTimerDuration;
                    _systemInformationService.Set("Health/TimerDurationAverageMax", _averageTimerDuration);
                }

                if (!_minTimerDuration.HasValue || _averageTimerDuration < _minTimerDuration.Value)
                {
                    _minTimerDuration = _averageTimerDuration;
                    _systemInformationService.Set("Health/TimerDurationAverageMin", _averageTimerDuration);
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
