using System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.OutdoorTemperature;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Automations
{
    public class AutomationFactory
    {
        private readonly ISchedulerService _schedulerService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDaylightService _daylightService;
        private readonly IOutdoorTemperatureService _outdoorTemperatureService;
        private readonly IComponentService _componentService;

        public AutomationFactory(
            ISchedulerService schedulerService, 
            IDateTimeService dateTimeService, 
            IDaylightService daylightService, 
            IOutdoorTemperatureService outdoorTemperatureService,
            IComponentService componentService)
        {
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));
            if (outdoorTemperatureService == null) throw new ArgumentNullException(nameof(outdoorTemperatureService));
            if (componentService == null) throw new ArgumentNullException(nameof(componentService));

            _schedulerService = schedulerService;
            _dateTimeService = dateTimeService;
            _daylightService = daylightService;
            _outdoorTemperatureService = outdoorTemperatureService;
            _componentService = componentService;
        }

        public ConditionalOnAutomation RegisterConditionalOnAutomation(IArea area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation =
                new ConditionalOnAutomation(
                    AutomationIdFactory.CreateIdFrom<ConditionalOnAutomation>(area),
                    _schedulerService,
                    _dateTimeService,
                    _daylightService);

            area.AddAutomation(automation);

            return automation;
        }

        public RollerShutterAutomation RegisterRollerShutterAutomation(IArea area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation = new RollerShutterAutomation(
                AutomationIdFactory.CreateIdFrom<RollerShutterAutomation>(area),
                _schedulerService,
                _dateTimeService,
                _daylightService,
                _outdoorTemperatureService,
                _componentService);
            
            area.AddAutomation(automation);

            return automation;
        }

        public TurnOnAndOffAutomation RegisterTurnOnAndOffAutomation(IArea area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            var automation =
                new TurnOnAndOffAutomation(
                    AutomationIdFactory.CreateIdFrom<TurnOnAndOffAutomation>(area),
                    _dateTimeService,
                    _schedulerService);

            area.AddAutomation(automation);

            return automation;
        }
    }
}
