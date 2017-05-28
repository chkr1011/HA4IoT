using System;
using System.Linq;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Hardware.RaspberryPi;
using HA4IoT.Contracts.Health.Configuration;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Health
{
    [ApiServiceClass(typeof(HealthService))]
    public class HealthService : ServiceBase, IHealthService
    {
        private readonly double[] _timerDurations = new double[1000];

        private int _timerDurationsIndex;
        private double? _averageTimerDuration;
        private double? _maxTimerDuration;
        private double? _minTimerDuration;

        private readonly ILogger _log;

        public HealthService(
            IConfigurationService configurationService,
            IController controller,
            ITimerService timerService,
            IGpioService gpioService,
            IDateTimeService dateTimeService,
            ISystemInformationService systemInformationService,
            ILogService logService)
        {
            if (configurationService == null) throw new ArgumentNullException(nameof(configurationService));
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (timerService == null) throw new ArgumentNullException(nameof(timerService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));

            _log = logService?.CreatePublisher(nameof(HealthService)) ?? throw new ArgumentNullException(nameof(logService));

            systemInformationService.Set("Health/SystemTime", () => dateTimeService.Now);
            systemInformationService.Set("Health/TimerDuration/Average", () => _averageTimerDuration);
            systemInformationService.Set("Health/TimerDuration/AverageMax", () => _maxTimerDuration);
            systemInformationService.Set("Health/TimerDuration/AverageMin", () => _minTimerDuration);
            systemInformationService.Set("Health/StartupTimestamp", dateTimeService.Now);
            systemInformationService.Set("Log/Errors/Count", () => logService.ErrorsCount);
            systemInformationService.Set("Log/Warnings/Count", () => logService.WarningsCount);

            controller.StartupCompleted += (sender, args) =>
            {
                _log.Info("Startup completed after " + args.Duration);
                systemInformationService.Set("Health/StartupDuration", args.Duration);
            };

            controller.StartupFailed += (sender, args) =>
            {
                _log.Error(args.Exception, "Startup failed after " + args.Duration);
            };

            timerService.Tick += UpdateStatistics;

            var configuration = configurationService.GetConfiguration<HealthServiceConfiguration>("HealthServiceConfiguration");
            if (configuration.StatusLed.Gpio.HasValue)
            {
                var gpio = gpioService.GetOutput(configuration.StatusLed.Gpio.Value);
                if (configuration.StatusLed.IsInverted)
                {
                    gpio = gpio.WithInvertedState();
                }

                var statusLed = new StatusLed(gpio);
                timerService.Tick += (s, e) => statusLed.Update();
            }
        }

        [ApiMethod]
        public void Reset(IApiCall apiCall)
        {
            ResetStatistics();
        }

        private void ResetStatistics()
        {
            _minTimerDuration = null;
            _maxTimerDuration = null;
            _averageTimerDuration = null;
        }

        private void UpdateStatistics(object sender, TimerTickEventArgs e)
        {
            _timerDurations[_timerDurationsIndex] = e.ElapsedTime.TotalMilliseconds;
            _timerDurationsIndex++;

            if (_timerDurationsIndex < _timerDurations.Length)
            {
                return;
            }

            _timerDurationsIndex = 0;

            _averageTimerDuration = _timerDurations.Sum() / _timerDurations.Length;

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
}
