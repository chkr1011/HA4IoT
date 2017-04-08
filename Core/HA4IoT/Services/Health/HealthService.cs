using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Core;

namespace HA4IoT.Services.Health
{
    [ApiServiceClass(typeof(HealthService))]
    public class HealthService : ServiceBase, IHealthService
    {
        private readonly ControllerOptions _controllerOptions;
        private readonly IGpioService _pi2GpioService;
        private readonly ITimerService _timerService;
        private readonly ISystemInformationService _systemInformationService;

        private readonly List<int> _timerDurations = new List<int>(100);
        private float? _averageTimerDuration;
        private float? _maxTimerDuration;
        private float? _minTimerDuration;

        public HealthService(
            ControllerOptions controllerOptions, 
            IGpioService pi2GpioService,
            ITimerService timerService, 
            ISystemInformationService systemInformationService)
        {
            if (controllerOptions == null) throw new ArgumentNullException(nameof(controllerOptions));
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));

            _controllerOptions = controllerOptions;
            _pi2GpioService = pi2GpioService;
            _timerService = timerService;
            _systemInformationService = systemInformationService;
        }

        public override void Startup()
        {
            Task.Factory.StartNew(BlinkLed, TaskCreationOptions.LongRunning);
            _timerService.Tick += Tick;
        }

        private async void BlinkLed()
        {
            if (!_controllerOptions.StatusLedGpio.HasValue)
            {
                return;
            }

            var led = _pi2GpioService.GetOutput(_controllerOptions.StatusLedGpio.Value);
            led.Write(BinaryState.Low);

            var ledState = false;

            while (true)
            {
                if (ledState)
                {
                    led.Write(BinaryState.High);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
                else
                {
                    led.Write(BinaryState.Low);
                    await Task.Delay(TimeSpan.FromMilliseconds(200));
                }

                ledState = !ledState;
            }
        }

        [ApiMethod]
        public void Reset(IApiContext apiContext)
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
            _timerDurations.Add((int)e.ElapsedTime.TotalMilliseconds);
            if (_timerDurations.Count == _timerDurations.Capacity)
            {
                _averageTimerDuration = _timerDurations.Sum() / (float)_timerDurations.Count;
                _timerDurations.Clear();

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
    }
}
