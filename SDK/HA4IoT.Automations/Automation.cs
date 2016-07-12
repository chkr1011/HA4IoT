using System;
using System.Collections.Generic;
using HA4IoT.Conditions;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Conditions;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Automations
{
    public class Automation : AutomationBase
    {
        private readonly ConditionsValidator _conditionsValidator;

        public Automation(AutomationId id)
            : base(id)
        {
            _conditionsValidator = new ConditionsValidator(Conditions);
        }

        public IList<RelatedCondition> Conditions { get; } = new List<RelatedCondition>();
        public IList<IAction> ActionsIfFulfilled { get; } = new List<IAction>();
        public IList<IAction> ActionsIfNotFulfilled { get; } = new List<IAction>();

        public Automation WithTrigger(ITrigger trigger)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            trigger.Attach(Trigger);
            return this;
        }

        public Automation WithCondition(ConditionRelation relation, Condition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            Conditions.Add(new RelatedCondition().WithCondition(condition).WithRelation(relation));
            return this;
        }

        public Automation WithActionIfConditionsFulfilled(IAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            ActionsIfFulfilled.Add(action);
            return this;
        }

        public Automation WithActionIfConditionsNotFulfilled(IAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            ActionsIfNotFulfilled.Add(action);
            return this;
        }

        public void Trigger()
        {
            if (_conditionsValidator.Validate() == ConditionState.Fulfilled)
            {
                foreach (var action in ActionsIfFulfilled)
                {
                    action.Execute();
                }
            }
            else
            {
                foreach (var action in ActionsIfNotFulfilled)
                {
                    action.Execute();
                }
            }            
        }
    }
}
