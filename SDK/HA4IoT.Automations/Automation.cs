using System;
using System.Collections.Generic;
using HA4IoT.Conditions;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking;
using HA4IoT.Contracts.Triggers;
using HA4IoT.Networking;

namespace HA4IoT.Automations
{
    public class Automation : AutomationBase<AutomationSettings>
    {
        private readonly ConditionsValidator _conditionsValidator;
        
        public IList<RelatedCondition> Conditions { get; } = new List<RelatedCondition>();
        public IList<Action> ActionsIfFulfilled { get; } = new List<Action>();
        public IList<Action> ActionsIfNotFulfilled { get; } = new List<Action>();

        public Automation(AutomationId id, IHomeAutomationTimer timer, IHttpRequestController httpApiController, ILogger logger)
            : base(id)
        {
            if (timer == null) throw new ArgumentNullException(nameof(timer));

            Timer = timer;
            _conditionsValidator = new ConditionsValidator(Conditions);

            Settings = new AutomationSettings(id, httpApiController, logger);
        }

        protected IHomeAutomationTimer Timer { get; }

        public Automation WithActionIfFulfilled(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            ActionsIfFulfilled.Add(action);
            return this;
        }

        public Automation WithActionIfNotFulfilled(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            ActionsIfNotFulfilled.Add(action);
            return this;
        }

        public Automation WithCondition(ConditionRelation relation, Condition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            Conditions.Add(new RelatedCondition().WithCondition(condition).WithRelation(relation));
            return this;
        }

        public void Trigger()
        {
            if (_conditionsValidator.Validate() == ConditionState.Fulfilled)
            {
                foreach (var action in ActionsIfFulfilled)
                {
                    action();
                }
            }
            else
            {
                foreach (var action in ActionsIfNotFulfilled)
                {
                    action();
                }
            }            
        }

        public Automation WithAutoTrigger(TimeSpan interval)
        {
            Timer.Every(interval).Do(Trigger);
            return this;
        }

        public Automation WithTrigger(ITrigger trigger)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            trigger.Attach(Trigger);
            return this;
        }
    }
}
