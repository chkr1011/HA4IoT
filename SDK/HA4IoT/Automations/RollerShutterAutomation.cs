using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Automations
{
    public class RollerShutterAutomation : AutomationBase
    {
        private readonly List<ComponentId> _rollerShutters = new List<ComponentId>();

        private readonly INotificationService _notificationService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDaylightService _daylightService;
        private readonly IOutdoorTemperatureService _outdoorTemperatureService;
        private readonly IComponentService _componentService;
        private readonly ISettingsService _settingsService;

        private bool _maxOutsideTemperatureApplied;
        private bool _autoOpenIsApplied;
        private bool _autoCloseIsApplied;
        
        public RollerShutterAutomation(
            AutomationId id, 
            INotificationService notificationService,
            ISchedulerService schedulerService,
            IDateTimeService dateTimeService,
            IDaylightService daylightService,
            IOutdoorTemperatureService outdoorTemperatureService,
            IComponentService componentService,
            ISettingsService settingsService)
            : base(id)
        {
            if (notificationService == null) throw new ArgumentNullException(nameof(notificationService));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));
            if (outdoorTemperatureService == null) throw new ArgumentNullException(nameof(outdoorTemperatureService));
            if (componentService == null) throw new ArgumentNullException(nameof(componentService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _notificationService = notificationService;
            _dateTimeService = dateTimeService;
            _daylightService = daylightService;
            _outdoorTemperatureService = outdoorTemperatureService;
            _componentService = componentService;
            _settingsService = settingsService;
            _componentService = componentService;

            settingsService.CreateSettingsMonitor<RollerShutterAutomationSettings>(Id, s => Settings = s);

            // TODO: Consider timer service here.
            schedulerService.RegisterSchedule("RollerShutterAutomation-" + Guid.NewGuid(), TimeSpan.FromMinutes(1), PerformPendingActions);
        }

        public RollerShutterAutomationSettings Settings { get; private set; }

        public RollerShutterAutomation WithRollerShutters(params IRollerShutter[] rollerShutters)
        {
            if (rollerShutters == null) throw new ArgumentNullException(nameof(rollerShutters));

            _rollerShutters.AddRange(rollerShutters.Select(rs => rs.Id));
            return this;
        }

        public RollerShutterAutomation WithRollerShutters(IList<IRollerShutter> rollerShutters)
        {
            if (rollerShutters == null) throw new ArgumentNullException(nameof(rollerShutters));

            _rollerShutters.AddRange(rollerShutters.Select(rs => rs.Id));
            return this;
        }

        public void PerformPendingActions()
        {
            if (!Settings.IsEnabled)
            {
                return;
            }

            if (!_maxOutsideTemperatureApplied && TooHotIsAffected())
            {
                _maxOutsideTemperatureApplied = true;

                _notificationService.CreateInformation($"Closing roller shutter because outside temperature reaches {Settings.AutoCloseIfTooHotTemperaure}°C.");
                SetStates(RollerShutterStateId.MovingDown);

                return;
            }

            // TODO: Add check for heavy hailing

            bool autoOpenIsInRange = GetIsDayCondition().IsFulfilled();
            bool autoCloseIsInRange = !autoOpenIsInRange;

            if (!_autoOpenIsApplied && autoOpenIsInRange)
            {
                PerformPendingSunriseActions();
            }
            else if (!_autoCloseIsApplied && autoCloseIsInRange)
            {
                PerformPendingSunsetActions();
            }
        }

        private void PerformPendingSunriseActions()
        {
            if (DoNotOpenDueToTimeIsAffected())
            {
                return;
            }

            if (TooColdIsAffected())
            {
                _notificationService.CreateInformation($"Cancelling opening because outside temperature is lower than {Settings.SkipIfFrozenTemperature}°C.");

                _autoOpenIsApplied = true;
                _autoCloseIsApplied = false;

                return;
            }

            if (TooHotIsAffected())
            {
                _notificationService.CreateInformation($"Cancelling opening because outside temperature is higher than {Settings.AutoCloseIfTooHotTemperaure}°C.");

                _autoOpenIsApplied = true;
                _autoCloseIsApplied = false;
                _maxOutsideTemperatureApplied = true;

                return;
            }

            if (Settings.SkipNextOpenOnSunrise)
            {
                Settings.SkipNextOpenOnSunrise = false;
                _settingsService.SetSettings(Id, Settings);
                _notificationService.CreateInformation("Skipped sunrise this time.");
            }
            else
            {
                SetStates(RollerShutterStateId.MovingUp);
                _notificationService.CreateInformation("Applied sunrise");
            }

            _autoOpenIsApplied = true;
            _autoCloseIsApplied = false;

            _maxOutsideTemperatureApplied = false;
        }

        private void PerformPendingSunsetActions()
        {
            if (TooColdIsAffected())
            {
                _notificationService.CreateInformation($"Cancelling closing because outside temperature is lower than {Settings.SkipIfFrozenTemperature}°C.");
            }
            else
            {
                if (Settings.SkipNextCloseOnSunset)
                {
                    Settings.SkipNextCloseOnSunset = false;
                    _settingsService.SetSettings(Id, Settings);
                    _notificationService.CreateInformation("Skipped sunset this time.");
                }
                else
                {
                    SetStates(RollerShutterStateId.MovingDown);
                    _notificationService.CreateInformation("Applied sunset");
                }
            }

            _autoCloseIsApplied = true;
            _autoOpenIsApplied = false;

            _notificationService.CreateInformation("Applied sunset");

        }

        private bool DoNotOpenDueToTimeIsAffected()
        {
            if (Settings.SkipBeforeTimestampIsEnabled &&
                Settings.SkipBeforeTimestamp > _dateTimeService.Time)
            {
                return true;
            }

            return false;
        }

        private bool TooHotIsAffected()
        {
            if (Settings.AutoCloseIfTooHotIsEnabled && 
                _outdoorTemperatureService.OutdoorTemperature > Settings.AutoCloseIfTooHotTemperaure)
            {
                return true;
            }

            return false;
        }

        private bool TooColdIsAffected()
        {
            if (Settings.SkipIfFrozenIsEnabled &&
                _outdoorTemperatureService.OutdoorTemperature < Settings.SkipIfFrozenTemperature)
            {
                return true;
            }

            return false;
        }

        private IsDayCondition GetIsDayCondition()
        {
            var condition = new IsDayCondition(_daylightService, _dateTimeService);
            condition.WithStartAdjustment(Settings.OpenOnSunriseOffset);
            condition.WithEndAdjustment(Settings.CloseOnSunsetOffset);

            return condition;
        }

        private void SetStates(ComponentState state)
        {
            foreach (var rollerShutter in _rollerShutters)
            {
                _componentService.GetComponent<IRollerShutter>(rollerShutter).SetState(state);
            }
        }
    }
}
