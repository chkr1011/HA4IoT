using System;
using HA4IoT.Actuators.Triggers;
using HA4IoT.Conditions;
using HA4IoT.Conditions.Specialized;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.WeatherStation;

namespace HA4IoT.Automations
{
    public class ConditionalOnAutomation : Automation
    {
        private readonly IHomeAutomationTimer _timer;

        public ConditionalOnAutomation(AutomationId id, IHomeAutomationTimer timer, IApiController apiController, ILogger logger) 
            : base(id, apiController, logger)
        {
            _timer = timer;

            WithTrigger(new IntervalTrigger(TimeSpan.FromMinutes(1), timer));
        }

        public ConditionalOnAutomation WithOnAtNightRange(IWeatherStation weatherStation)
        {
            var nightCondition = new TimeRangeCondition(_timer).WithStart(() => weatherStation.Daylight.Sunset).WithEnd(() => weatherStation.Daylight.Sunrise);
            WithCondition(ConditionRelation.And, nightCondition);

            return this;
        }

        public ConditionalOnAutomation WithOffBetweenRange(TimeSpan from, TimeSpan until)
        {
            WithCondition(ConditionRelation.AndNot, new TimeRangeCondition(_timer).WithStart(() => from).WithEnd(() => until));

            return this;
        }

        public ConditionalOnAutomation WithActuator(IBinaryStateOutputActuator actuator)
        {
            WithActionIfConditionsFulfilled(actuator.GetTurnOnAction());
            WithActionIfConditionsNotFulfilled(actuator.GetTurnOffAction());

            return this;
        }
    }
}
