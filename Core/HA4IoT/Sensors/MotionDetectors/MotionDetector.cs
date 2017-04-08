using System;
using HA4IoT.Commands;
using HA4IoT.Components;
using HA4IoT.Contracts.Adapters;
using HA4IoT.Contracts.Commands;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Sensors;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Triggers;

namespace HA4IoT.Sensors.MotionDetectors
{
    public class MotionDetector : ComponentBase, IMotionDetector
    {
        private readonly ISettingsService _settingsService;
        private readonly ISchedulerService _schedulerService;

        private IDelayedAction _autoEnableAction;
        private MotionDetectionStateValue _motionDetectionState = MotionDetectionStateValue.Idle;

        public MotionDetector(string id, IMotionDetectorAdapter adapter, ISchedulerService schedulerService, ISettingsService settingsService)
            : base(id)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));

            adapter.MotionDetectionBegin += (s, e) => UpdateState(MotionDetectionStateValue.MotionDetected);
            adapter.MotionDetectionEnd += (s, e) => UpdateState(MotionDetectionStateValue.Idle);

            settingsService.CreateSettingsMonitor<MotionDetectorSettings>(this, s =>
            {
                Settings = s.NewSettings;

                if (s.OldSettings != null && s.OldSettings.IsEnabled != s.NewSettings.IsEnabled)
                {
                    HandleIsEnabledStateChanged();
                }
            });
        }

        public MotionDetectorSettings Settings { get; private set; }

        public ITrigger MotionDetectedTrigger { get; } = new Trigger();

        public ITrigger MotionDetectionCompletedTrigger { get; } = new Trigger();

        public override IComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection()
                .With(new MotionDetectionFeature());
        }

        public override IComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection()
                .With(new MotionDetectionState(_motionDetectionState));
        }

        public override void ExecuteCommand(ICommand command)
        {
            var commandExecutor = new CommandExecutor();
            commandExecutor.Register<ResetCommand>();
            commandExecutor.Execute(command);
        }

        private void UpdateState(MotionDetectionStateValue state)
        {
            if (state == _motionDetectionState)
            {
                return;
            }

            if (state == MotionDetectionStateValue.MotionDetected && !Settings.IsEnabled)
            {
                return;
            }

            var oldState = GetState();
            _motionDetectionState = state;
            OnStateChanged(oldState);

            if (state == MotionDetectionStateValue.MotionDetected)
            {
                ((Trigger)MotionDetectedTrigger).Execute();
            }
            else if (state == MotionDetectionStateValue.Idle)
            {
                ((Trigger)MotionDetectionCompletedTrigger).Execute();
            }
        }

        private void HandleIsEnabledStateChanged()
        {
            _autoEnableAction?.Cancel();

            if (!Settings.IsEnabled)
            {
                _autoEnableAction = _schedulerService.In(Settings.AutoEnableAfter, () => _settingsService.SetComponentEnabledState(this, true));
            }
        }
    }
}