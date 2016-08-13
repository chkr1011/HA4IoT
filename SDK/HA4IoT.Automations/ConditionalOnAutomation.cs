using System;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Automations
{
    public class ConditionalOnAutomation : Automation
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IDaylightService _daylightService;

        public ConditionalOnAutomation(AutomationId id, ISchedulerService schedulerService, IDateTimeService dateTimeService, IDaylightService daylightService) 
            : base(id)
        {
            if (dateTimeService == null) throw new ArgumentNullException(nameof(dateTimeService));
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));

            _dateTimeService = dateTimeService;
            _daylightService = daylightService;
            
            WithTrigger(new IntervalTrigger(TimeSpan.FromMinutes(1), schedulerService));
        }

        public ConditionalOnAutomation WithOnAtNightRange()
        {
            var nightCondition = new TimeRangeCondition(_dateTimeService).WithStart(_daylightService.Sunset).WithEnd(_daylightService.Sunrise);
            WithCondition(ConditionRelation.And, nightCondition);

            return this;
        }

        public ConditionalOnAutomation WithOffBetweenRange(TimeSpan from, TimeSpan until)
        {
            WithCondition(ConditionRelation.AndNot, new TimeRangeCondition(_dateTimeService).WithStart(() => from).WithEnd(() => until));

            return this;
        }

        public ConditionalOnAutomation WithActuator(IStateMachine actuator)
        {
            if (actuator == null) throw new ArgumentNullException(nameof(actuator));

            WithActionIfConditionsFulfilled(actuator.GetTurnOnAction());
            WithActionIfConditionsNotFulfilled(actuator.GetTurnOffAction());

            return this;
        }
    }
}
