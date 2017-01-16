using System;
using System.Collections.Generic;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Sensors.MotionDetectors
{
    public class MotionDetector : SensorBase, IMotionDetector
    {
        private readonly ISchedulerService _schedulerService;
        private readonly Trigger _motionDetectedTrigger = new Trigger();
        private readonly Trigger _detectionCompletedTrigger = new Trigger();

        private TimedAction _autoEnableAction;

        public MotionDetector(ComponentId id, IMotionDetectorAdapter endpoint, ISchedulerService schedulerService, ISettingsService settingsService)
            : base(id)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _schedulerService = schedulerService;

            settingsService.CreateSettingsMonitor<MotionDetectorSettings>(Id, s => Settings = s);

            SetState(MotionDetectorStateId.Idle);

            endpoint.MotionDetected += (s, e) => UpdateState(MotionDetectorStateId.MotionDetected);
            endpoint.DetectionCompleted += (s, e) => UpdateState(MotionDetectorStateId.Idle);

            Settings.ValueChanged += (s, e) =>
            {
                if (e.SettingName == nameof(Settings.IsEnabled))
                {
                    HandleIsEnabledStateChanged();
                }
            };
        }

        public IMotionDetectorSettings Settings { get; private set; }

        public ITrigger GetMotionDetectedTrigger()
        {
            return _motionDetectedTrigger;
        }

        public ITrigger GetDetectionCompletedTrigger()
        {
            return _detectionCompletedTrigger;
        }

        public override IList<ComponentState> GetSupportedStates()
        {
            return new List<ComponentState> { MotionDetectorStateId.Idle, MotionDetectorStateId.MotionDetected };
        }

        public override void HandleApiCall(IApiContext apiContext)
        {
            var action = (string)apiContext.Parameter["Action"];

            if (action.Equals("detected", StringComparison.OrdinalIgnoreCase))
            {
                UpdateState(MotionDetectorStateId.MotionDetected);
            }
            else if (action.Equals("detectionCompleted", StringComparison.OrdinalIgnoreCase))
            {
                UpdateState(MotionDetectorStateId.Idle);
            }
        }

        protected void OnMotionDetected()
        {
            Log.Info(Id + ": Motion detected");
            _motionDetectedTrigger.Execute();
        }

        protected void OnDetectionCompleted()
        {
            Log.Verbose(Id + ": Detection completed");
            _detectionCompletedTrigger.Execute();
        }

        private void UpdateState(ComponentState newState)
        {
            if (!Settings.IsEnabled)
            {
                return;
            }

            SetState(newState);

            if (newState.Equals(MotionDetectorStateId.MotionDetected))
            {
                OnMotionDetected();
            }
            else
            {
                OnDetectionCompleted();
            }
        }

        private void HandleIsEnabledStateChanged()
        {
            if (!Settings.IsEnabled)
            {
                Log.Info(Id + ": Disabled for 1 hour");

                _autoEnableAction = _schedulerService.In(Settings.AutoEnableAfter).Execute(() => Settings.IsEnabled = true);
            }
            else
            {
                _autoEnableAction?.Cancel();
            }
        }
    }
}