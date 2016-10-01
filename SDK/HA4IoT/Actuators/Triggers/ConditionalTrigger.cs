using System;
using HA4IoT.Contracts.Conditions;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Actuators.Triggers
{
    public class ConditionalTrigger : TriggerBase
    {
        private ICondition _condition;

        public ConditionalTrigger WithTrigger(ITrigger trigger)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            trigger.Attach(ForwardTriggerEvent);
            return this;
        }

        public ConditionalTrigger WithCondition(ICondition condition)
        {
            _condition = condition;
            return this;
        }

        private void ForwardTriggerEvent()
        {
            if (_condition != null)
            {
                if (_condition.Validate() != ConditionState.Fulfilled)
                {
                    return;
                }
            }

            Execute();
        }
    }
}
