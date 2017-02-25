using System;
using HA4IoT.Components;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Triggers;

namespace HA4IoT.Sensors.MotionDetectors
{
    public class MotionDetector : ComponentBase, IMotionDetector
    {
        private readonly ISchedulerService _schedulerService;
        private readonly ISettingsService _settingsService;

        private TimedAction _autoEnableAction;
        private MotionDetectionStateValue _motionDetectionState = MotionDetectionStateValue.Idle;

        public MotionDetector(string id, IMotionDetectorAdapter adapter, ISchedulerService schedulerService, ISettingsService settingsService)
            : base(id)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            
            _schedulerService = schedulerService;
            _settingsService = settingsService;

            adapter.MotionDetectionBegin += (s, e) => UpdateState(MotionDetectionStateValue.MotionDetected);
            adapter.MotionDetectionEnd += (s, e) => UpdateState(MotionDetectionStateValue.Idle);

            settingsService.CreateComponentSettingsMonitor<MotionDetectorSettings>(Id, s =>
            {
                Settings = s;
            });
            
            Settings.ValueChanged += (s, e) =>
            {
                if (e.SettingName == nameof(Settings.IsEnabled))
                {
                    HandleIsEnabledStateChanged();
                }
            };
        }

        public MotionDetectorSettings Settings { get; private set; }

        public ITrigger MotionDetectedTrigger { get; } = new Trigger();

        public ITrigger MotionDetectionCompletedTrigger { get; } = new Trigger();
        
        public override ComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection()
                .With(new MotionDetectionFeature());
        }

        public override ComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection()
                .With(new MotionDetectionState(_motionDetectionState));
        }

        public override void InvokeCommand(ICommand command)
        {
            var commandInvoker = new CommandInvoker();

            commandInvoker.Invoke(command);
        }

        private void UpdateState(MotionDetectionStateValue state)
        {
            if (state == _motionDetectionState)
            {
                return;
            }

            if (!_settingsService.GetComponentSettings<MotionDetectorSettings>(Id).IsEnabled)
            {
                return;
            }

            var oldState = GetState();
            _motionDetectionState = state;
            OnStateChanged(oldState);

            if (state == MotionDetectionStateValue.MotionDetected)
            {
                Log.Info(Id + ": Motion detected");
                ((Trigger)MotionDetectedTrigger).Execute();
            }
            else if (state == MotionDetectionStateValue.Idle)
            {
                Log.Verbose(Id + ": Detection completed");
                ((Trigger)MotionDetectionCompletedTrigger).Execute();
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