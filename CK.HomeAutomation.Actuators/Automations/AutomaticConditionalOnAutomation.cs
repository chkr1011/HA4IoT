using System;
using System.Collections.Generic;
using CK.HomeAutomation.Actuators.Conditions;
using CK.HomeAutomation.Actuators.Conditions.Specialized;
using CK.HomeAutomation.Actuators.Contracts;
using CK.HomeAutomation.Core.Timer;
using CK.HomeAutomation.Hardware;

namespace CK.HomeAutomation.Actuators.Automations
{
    public class AutomaticConditionalOnAutomation : Automation
    {
        private readonly List<IBinaryStateOutputActuator> _actuators = new List<IBinaryStateOutputActuator>();

        public AutomaticConditionalOnAutomation(IHomeAutomationTimer timer) : base(timer)
        {
            WithAutoTrigger(TimeSpan.FromMinutes(1));

            WithActionIfFulfilled(TurnOn);
            WithActionIfNotFulfilled(TurnOff);
        }

        public AutomaticConditionalOnAutomation WithOnlyAtNightRange(IWeatherStation weatherStation)
        {
            var nightCondition = new TimeRangeCondition(Timer).WithStart(() => weatherStation.Daylight.Sunset).WithEnd(() => weatherStation.Daylight.Sunrise);
            WithCondition(ConditionRelation.And, nightCondition);

            return this;
        }

        public AutomaticConditionalOnAutomation WithOffBetweenRange(TimeSpan from, TimeSpan until)
        {
            WithCondition(ConditionRelation.AndNot, new TimeRangeCondition(Timer).WithStart(() => from).WithEnd(() => until));
            return this;
        }

        public AutomaticConditionalOnAutomation WithActuator(IBinaryStateOutputActuator actuator)
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
