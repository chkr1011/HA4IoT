using System;
using System.Collections.Generic;
using HA4IoT.Conditions;
using HA4IoT.Contracts.Actions;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Triggers;

namespace HA4IoT.Automations
{
    public class Automation : AutomationBase<AutomationSettings>
    {
        private readonly ConditionsValidator _conditionsValidator;

        public Automation(AutomationId id, IApiController apiController, ILogger logger)
            : base(id)
        {
            _conditionsValidator = new ConditionsValidator(Conditions);

            Settings = new AutomationSettings(id, apiController, logger);
        }

        public IList<RelatedCondition> Conditions { get; } = new List<RelatedCondition>();
        public IList<IActuatorAction> ActionsIfFulfilled { get; } = new List<IActuatorAction>();
        public IList<IActuatorAction> ActionsIfNotFulfilled { get; } = new List<IActuatorAction>();

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

        public Automation WithActionIfConditionsFulfilled(IActuatorAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            ActionsIfFulfilled.Add(action);
            return this;
        }

        public Automation WithActionIfConditionsNotFulfilled(IActuatorAction action)
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
