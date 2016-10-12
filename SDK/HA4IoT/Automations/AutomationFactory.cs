using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.Notifications;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.Resources;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Automations
{
    public class AutomationFactory
    {
        private readonly ISchedulerService _schedulerService;
        private readonly INotificationService _notificationService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDaylightService _daylightService;
        private readonly IOutdoorTemperatureService _outdoorTemperatureService;
        private readonly IComponentService _componentService;
        private readonly ISettingsService _settingsService;
        private readonly IResourceService _resourceService;

        public AutomationFactory(
            ISchedulerService schedulerService,
            INotificationService notificationService,
            IDateTimeService dateTimeService,
            IDaylightService daylightService,
            IOutdoorTemperatureService outdoorTemperatureService,
            IComponentService componentService,
            ISettingsService settingsService,
            IResourceService resourceService)
        {
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (notificationService == null) throw new ArgumentNullException(nameof(notificationService));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));
            if (outdoorTemperatureService == null) throw new ArgumentNullException(nameof(outdoorTemperatureService));
            if (componentService == null) throw new ArgumentNullException(nameof(componentService));
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));
            if (resourceService == null) throw new ArgumentNullException(nameof(resourceService));

            _schedulerService = schedulerService;
            _notificationService = notificationService;
            _dateTimeService = dateTimeService;
            _daylightService = daylightService;
            _outdoorTemperatureService = outdoorTemperatureService;
            _componentService = componentService;
            _settingsService = settingsService;
            _resourceService = resourceService;
        }

        public ConditionalOnAutomation RegisterConditionalOnAutomation(IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation =
                new ConditionalOnAutomation(
                    AutomationIdGenerator.Generate(area, id),
                    _schedulerService,
                    _dateTimeService,
                    _daylightService);

            area.AddAutomation(automation);

            return automation;
        }

        public RollerShutterAutomation RegisterRollerShutterAutomation(IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation = new RollerShutterAutomation(
                AutomationIdGenerator.Generate(area, id),
                _notificationService,
                _schedulerService,
                _dateTimeService,
                _daylightService,
                _outdoorTemperatureService,
                _componentService,
                _settingsService,
                _resourceService);

            area.AddAutomation(automation);

            return automation;
        }

        public TurnOnAndOffAutomation RegisterTurnOnAndOffAutomation(IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation =
                new TurnOnAndOffAutomation(
                    AutomationIdGenerator.Generate(area, id),
                    _dateTimeService,
                    _schedulerService,
                    _settingsService,
                    _daylightService);

            area.AddAutomation(automation);

            return automation;
        }
    }
}
