using System;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Automations
{
    public class ConditionalOnAutomation : Automation
    {
        private readonly IHomeAutomationTimer _timer;

        public ConditionalOnAutomation(AutomationId id, IHomeAutomationTimer timer) 
            : base(id)
        {
            _timer = timer;

            WithTrigger(new IntervalTrigger(TimeSpan.FromMinutes(1), timer));
        }

        public ConditionalOnAutomation WithOnAtNightRange(IDaylightService daylightService)
        {
            if (daylightService == null) throw new ArgumentNullException(nameof(daylightService));

            var nightCondition = new TimeRangeCondition(_timer).WithStart(daylightService.GetSunset).WithEnd(daylightService.GetSunrise);
            WithCondition(ConditionRelation.And, nightCondition);

            return this;
        }

        public ConditionalOnAutomation WithOffBetweenRange(TimeSpan from, TimeSpan until)
        {
            WithCondition(ConditionRelation.AndNot, new TimeRangeCondition(_timer).WithStart(() => from).WithEnd(() => until));

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
