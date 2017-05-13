using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Messaging;
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
        private readonly IMessageBrokerService _messageBroker;
        private readonly ISchedulerService _schedulerService;
        private readonly INotificationService _notificationService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDaylightService _daylightService;
        private readonly IOutdoorTemperatureService _outdoorTemperatureService;
        private readonly IComponentRegistryService _componentService;
        private readonly ISettingsService _settingsService;
        private readonly IResourceService _resourceService;

        public AutomationFactory(
            ISchedulerService schedulerService,
            INotificationService notificationService,
            IDateTimeService dateTimeService,
            IDaylightService daylightService,
            IOutdoorTemperatureService outdoorTemperatureService,
            IComponentRegistryService componentService,
            ISettingsService settingsService,
            IResourceService resourceService,
            IMessageBrokerService messageBroker)
        {
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _daylightService = daylightService ?? throw new ArgumentNullException(nameof(daylightService));
            _outdoorTemperatureService = outdoorTemperatureService ?? throw new ArgumentNullException(nameof(outdoorTemperatureService));
            _componentService = componentService ?? throw new ArgumentNullException(nameof(componentService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
        }

        public ConditionalOnAutomation RegisterConditionalOnAutomation(IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation =
                new ConditionalOnAutomation(
                    $"{area.Id}.{id}",
                    _schedulerService,
                    _dateTimeService,
                    _daylightService);

            area.RegisterAutomation(automation);

            return automation;
        }

        public RollerShutterAutomation RegisterRollerShutterAutomation(IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation = new RollerShutterAutomation(
                $"{area.Id}.{id}",
                _notificationService,
                _schedulerService,
                _dateTimeService,
                _daylightService,
                _outdoorTemperatureService,
                _componentService,
                _settingsService,
                _resourceService);

            area.RegisterAutomation(automation);

            return automation;
        }

        public TurnOnAndOffAutomation RegisterTurnOnAndOffAutomation(IArea area, Enum id)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation =
                new TurnOnAndOffAutomation(
                    $"{area.Id}.{id}",
                    _dateTimeService,
                    _schedulerService,
                    _settingsService,
                    _daylightService,
                    _messageBroker);

            area.RegisterAutomation(automation);

            return automation;
        }
    }
}
