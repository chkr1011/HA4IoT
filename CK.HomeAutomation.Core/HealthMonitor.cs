using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using Windows.Devices.Gpio;
using CK.HomeAutomation.Core.Timer;
using CK.HomeAutomation.Networking;

namespace CK.HomeAutomation.Core
{
    public class HealthMonitor
    {
        private readonly List<int> _durations = new List<int>(100);
        private readonly Timeout _ledTimeout = new Timeout();
        private readonly DateTime _startedDate = DateTime.Now;
        private readonly GpioPin _statusLed;
        private float? _averageTimerDuration;

        private bool _ledState;
        private float? _maxTimerDuration;

        private float? _minTimerDuration;

        public HealthMonitor(GpioPin statusLed, IHomeAutomationTimer timer, IHttpRequestController httpApiController)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));
            if (httpApiController == null) throw new ArgumentNullException(nameof(httpApiController));

            _statusLed = statusLed;
            if (statusLed != null)
            {
                _ledTimeout.Start(TimeSpan.FromMilliseconds(1));
            }

            timer.Tick += Tick;
            httpApiController.Handle(HttpMethod.Get, "health").Using(c => c.Response.Result = ApiGet());
            httpApiController.Handle(HttpMethod.Post, "health").WithSegment("reset").Using(c => ResetStatistics());
        }

        public JsonObject ApiGet()
        {
            var status = new JsonObject();

            status.SetNamedValue("timerMin",
                _minTimerDuration.HasValue ? JsonValue.CreateNumberValue(_minTimerDuration.Value) : JsonValue.CreateNullValue());

            status.SetNamedValue("timerMax",
                _maxTimerDuration.HasValue ? JsonValue.CreateNumberValue(_maxTimerDuration.Value) : JsonValue.CreateNullValue());

            status.SetNamedValue("timerAverage",
                _averageTimerDuration.HasValue ? JsonValue.CreateNumberValue(_averageTimerDuration.Value) : JsonValue.CreateNullValue());

            status.SetNamedValue("uptime", JsonValue.CreateStringValue((DateTime.Now - _startedDate).ToString()));

            return status;
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
                _statusLed.Write(GpioPinValue.High);
                _ledTimeout.Start(TimeSpan.FromSeconds(5));
            }
            else
            {
                _statusLed.Write(GpioPinValue.Low);
                _ledTimeout.Start(TimeSpan.FromMilliseconds(200));
            }

            _ledState = !_ledState;
        }
    }
}
