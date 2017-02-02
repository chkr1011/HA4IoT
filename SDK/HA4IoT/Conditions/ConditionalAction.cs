using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Conditions;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Conditions
{
    public class ConditionalAction : IAction
    {
        private readonly List<IAction> _actions = new List<IAction>(); 
        private readonly ConditionsValidator _conditionsValidator = new ConditionsValidator(new List<RelatedCondition>());

        public ConditionState ExpectedStateForExecution { get; set; } = ConditionState.Fulfilled;

        public void AddAction(IAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _actions.Add(action);
        }

        public void AddCondition(ConditionRelation relation, Condition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            _conditionsValidator.WithCondition(relation, condition);
        }

        public void Execute()
        {
            if (_conditionsValidator.Validate() != ExpectedStateForExecution)
            {
                return;
            }

            foreach (var action in _actions)
            {
                action.Execute();
            }
        }

        public ConditionalAction WithAction(IAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            AddAction(action);
            return this;
        }

        public ConditionalAction WithCondition(ConditionRelation relation, Condition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            AddCondition(relation, condition);
            return this;
        }
    }
}
