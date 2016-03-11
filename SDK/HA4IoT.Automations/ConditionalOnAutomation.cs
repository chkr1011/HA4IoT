using System;
using System.Collections.Generic;
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
        private readonly List<IBinaryStateOutputActuator> _actuators = new List<IBinaryStateOutputActuator>();

        public ConditionalOnAutomation(AutomationId id, IHomeAutomationTimer timer, IApiController apiController, ILogger logger) 
            : base(id, timer, apiController, logger)
        {
            WithAutoTrigger(TimeSpan.FromMinutes(1));

            WithActionIfFulfilled(TurnOn);
            WithActionIfNotFulfilled(TurnOff);
        }

        public ConditionalOnAutomation WithOnAtNightRange(IWeatherStation weatherStation)
        {
            var nightCondition = new TimeRangeCondition(Timer).WithStart(() => weatherStation.Daylight.Sunset).WithEnd(() => weatherStation.Daylight.Sunrise);
            WithCondition(ConditionRelation.And, nightCondition);

            return this;
        }

        public ConditionalOnAutomation WithOffBetweenRange(TimeSpan from, TimeSpan until)
        {
            WithCondition(ConditionRelation.AndNot, new TimeRangeCondition(Timer).WithStart(() => from).WithEnd(() => until));
            return this;
        }

        public ConditionalOnAutomation WithActuator(IBinaryStateOutputActuator actuator)
        {
            _actuators.Add(actuator);
            return this;
        }

        private void TurnOn()
        {
            foreach (var actuator in _actuators)
            {
                actuator.SetState(BinaryActuatorState.On);
            }
        }

        private void TurnOff()
        {
            foreach (var actuator in _actuators)
            {
                actuator.SetState(BinaryActuatorState.Off);
            }
        }
    }
}
