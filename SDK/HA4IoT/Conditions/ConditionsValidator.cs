using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Conditions;

namespace HA4IoT.Conditions
{
    public class ConditionsValidator
    {
        private ConditionState _defaultState = ConditionState.Fulfilled;

        public ConditionsValidator()
        {
            Conditions = new List<RelatedCondition>();
        }

        public ConditionsValidator(IList<RelatedCondition> conditions)
        {
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));

            Conditions = conditions;
        }

        public IList<RelatedCondition> Conditions { get; }

        public ConditionsValidator WithCondition(ConditionRelation relation, Condition condition)
        {
            Conditions.Add(new RelatedCondition().WithRelation(relation).WithCondition(condition));
            return this;
        }

        public ConditionsValidator WithDefaultState(ConditionState state)
        {
            _defaultState = state;
            return this;
        }

        public ConditionState Validate()
        {
            if (!Conditions.Any())
            {
                return _defaultState;
            }

            bool anyIsFulfilled = false;
            foreach (var relatedCondition in Conditions)
            {
                ConditionState relatedConditionState = GetConditionState(relatedCondition, _defaultState);
                if (relatedConditionState == ConditionState.NotFulfilled && relatedCondition.Relation == ConditionRelation.And)
                {
                    return ConditionState.NotFulfilled;
                }

                if (relatedConditionState == ConditionState.NotFulfilled && relatedCondition.Relation == ConditionRelation.AndNot)
                {
                    return ConditionState.NotFulfilled;
                }

                if (relatedConditionState == ConditionState.Fulfilled)
                {
                    anyIsFulfilled = true;
                }
            }

            if (anyIsFulfilled)
            {
                return ConditionState.Fulfilled;
            }

            return ConditionState.NotFulfilled;
        }

        private ConditionState GetConditionState(RelatedCondition relatedCondition, ConditionState comparisonState)
        {
            bool firstIsFulfilled = comparisonState == ConditionState.Fulfilled;
            bool secondIsFulfilled = relatedCondition.Validate() == ConditionState.Fulfilled;

            switch (relatedCondition.Relation)
            {
                case ConditionRelation.And:
                    {
                        if (firstIsFulfilled && secondIsFulfilled)
                        {
                            return ConditionState.Fulfilled;
                        }

                        break;
                    }

                case ConditionRelation.Or:
                    {
                        if (firstIsFulfilled || secondIsFulfilled)
                        {
                            return ConditionState.Fulfilled;
                        }

                        break;
                    }

                case ConditionRelation.AndNot:
                    {
                        if (firstIsFulfilled && !secondIsFulfilled)
                        {
                            return ConditionState.Fulfilled;
                        }

                        break;
                    }

                case ConditionRelation.OrNot:
                    {
                        if (firstIsFulfilled || !secondIsFulfilled)
                        {
                            return ConditionState.Fulfilled;
                        }

                        break;
                    }
            }

            return ConditionState.NotFulfilled;
        }
    }
}
