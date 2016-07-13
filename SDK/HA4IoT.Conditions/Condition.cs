using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Conditions;

namespace HA4IoT.Conditions
{
    public class Condition : ICondition
    {
        private readonly ConditionsValidator _relatedConditionsValidator;

        public Func<ConditionState> Expression { get; private set; } = () => ConditionState.Fulfilled;

        public IList<RelatedCondition> RelatedConditions { get; } = new List<RelatedCondition>();

        public Condition()
        {
            _relatedConditionsValidator = new ConditionsValidator(RelatedConditions);
        }

        public ConditionState Validate()
        {
            ConditionState thisState = Expression();

            if (RelatedConditions.Any())
            {
                _relatedConditionsValidator.WithDefaultState(thisState);
                return _relatedConditionsValidator.Validate();
            }

            return thisState;
        }

        public Condition WithRelatedCondition(ConditionRelation relation, Condition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            RelatedConditions.Add(new RelatedCondition().WithCondition(condition).WithRelation(relation));
            return this;
        }
        
        public Condition WithExpression(Func<ConditionState> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            Expression = expression;
            return this;
        }

        public Condition WithExpression(Func<bool> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            Expression = () => expression() ? ConditionState.Fulfilled : ConditionState.NotFulfilled;
            return this;
        }
    }
}
